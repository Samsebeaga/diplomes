using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Text.RegularExpressions;

namespace CGTUnity.Fungus.SaveSystem
{
    /// <summary>
    /// Reads save data from disk and encodes it into a GameSaveData object(s).
    /// </summary>
    [CreateAssetMenu(fileName = "NewSaveReader", menuName = "Fungus/SaveSystem/SaveReader")]
    public class SaveReader : MonoBehaviour
    {
        #region Fields

        [Tooltip("Set this to true if you want it to read non-plaintext-written save data. Experimental.")]
        [SerializeField] protected bool readEncrypted;

        [Tooltip("Encoding used when reading save files.")]
        [SerializeField] protected string encodingName = "UTF-8";

        [Tooltip("Prefix for save file names.")]
        [SerializeField] protected string savePrefix = "save";

        [Tooltip("Extension for save files.")]
        [SerializeField] protected string fileExtension = "json";

        protected System.Text.Encoding actualEncoding;

        protected string fileNameFormat;

        public UnityAction<GameSaveData, string, string> GameSaveRead = delegate { };

        #endregion

        #region Methods

        protected virtual void OnEnable()
        {
            actualEncoding = System.Text.Encoding.GetEncoding(encodingName);

            // Формируем регулярное выражение для проверки имени файла
            var savePrefixMatch = Regex.Escape(savePrefix);
            var prefixSeparator = "_0";
            var saveDataNumber = @"\d+";
            var fileExtensionMatch = Regex.Escape("." + fileExtension);
            fileNameFormat = $"^{savePrefixMatch}{prefixSeparator}{saveDataNumber}{fileExtensionMatch}$";
        }


        /// <summary>
        /// Reads a save file at the passed filePath, returning a GameSaveData if appropriate.
        /// </summary>
        public virtual GameSaveData ReadOneFromDisk(string filePath)
        {
            // Safety.
            if (!File.Exists(filePath))
            {
                var messageFormat = "Read error: file path doesn't exist. {0}";
                var message = string.Format(messageFormat, filePath);
                throw new System.ArgumentException(message);
            }

            // Read the file's contents into a json string...
            var jsonSaveData = "";

            if (!readEncrypted)
            {
                jsonSaveData = File.ReadAllText(filePath, actualEncoding);
            }
            else
            {
                using (Stream fileStream = File.Open(filePath, FileMode.Open))
                {
                    using (BinaryReader reader = new BinaryReader(fileStream, actualEncoding))
                    {
                        while (reader.PeekChar() != -1)
                        {
                            jsonSaveData = string.Concat(jsonSaveData, reader.ReadString());
                        }
                    }
                }
            }

            // ... then make sure it worked as intended.
            var saveData = JsonUtility.FromJson<GameSaveData>(jsonSaveData);
            saveData.OnDeserialize();
            ValidateReadSaveData(saveData, filePath);

            // Alert listeners
            var fileNameIndex = filePath.LastIndexOf('\\') + 1;
            var fileName = filePath.Substring(fileNameIndex);
            Signals.GameSaveRead.Invoke(saveData, filePath, fileName);
            GameSaveRead.Invoke(saveData, filePath, fileName);
            return saveData;
        }

        /// <summary>
        /// Reads GameSaveDatas from the passed directory, and into the passed collection
        /// (if appropriate).
        /// </summary>
        public virtual void ReadAllFromDisk(string saveDir, ICollection<GameSaveData> outputTo = null)
        {
            // Safety.
            if (!Directory.Exists(saveDir))
            {
                var messageFormat = 
                @"Could not read saves from {0}; that directory does not exist.";
                var message = string.Format(messageFormat, saveDir);
                throw new System.ArgumentException(message);
            }

            // Get all the locations for the files this is meant to read.
            var directories = new List<string>(Directory.GetFiles(saveDir));
            directories.RemoveAll(ShouldBeIgnored);
            
            // Extract GameSaveDatas from the files, adding them to the passed output
            // container if appropriate.
            var directory = "";
            var passOutput = outputTo != null;

            for (int i = 0; i < directories.Count; i++)
            {
                directory = directories[i];
                var saveData = ReadOneFromDisk(directory);
                if (passOutput)
                    outputTo.Add(saveData);
            }

        }

        protected virtual bool ShouldBeIgnored(string filePath)
        {
            // All depends on the file name fitting this reader's pattern thereof.
            var fileNameIndex = filePath.LastIndexOf('/') + 1;
            var fileName = filePath.Substring(fileNameIndex);

            return !Regex.IsMatch(fileName, fileNameFormat);
        }

        protected virtual void ValidateReadSaveData(GameSaveData readData, string filePath)
        {
            // If the essential fields aren't filled, then there was a read error.
            if (string.IsNullOrEmpty(readData.SceneName) ||
                readData.Items.Count == 0)
                throw new System.FormatException("Save data at " + filePath + " is not valid.");

        }
        #endregion
    }
}