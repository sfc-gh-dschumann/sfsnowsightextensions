// Copyright (c) 2021 Snowflake Inc. All rights reserved.

// Licensed under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License.  You may obtain a copy of the License at

//   http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied.  See the License for the
// specific language governing permissions and limitations
// under the License.

using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Snowflake.Powershell
{
    /// <summary>
    /// Helper functions for dealing with Folders and Files
    /// </summary>
    public class FileIOHelper
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private static Logger loggerConsole = LogManager.GetLogger("Snowflake.Powershell.Console");

        #region Basic file and folder reading and writing

        public static bool CreateFolder(string folderPath)
        {
            try
            {
                if (!Directory.Exists(folderPath))
                {
                    logger.Info("Creating folder {0}", folderPath);

                    Directory.CreateDirectory(folderPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Unable to create folder {0}", folderPath);
                logger.Error(ex);

                return false;
            }
        }

        public static bool CreateFolderForFile(string filePath)
        {
            return CreateFolder(Path.GetDirectoryName(filePath));
        }

        public static bool DeleteFolder(string folderPath)
        {
            int tryNumber = 1;

            do
            {
                try
                {
                    if (Directory.Exists(folderPath))
                    {
                        logger.Info("Deleting folder {0}, try #{1}", folderPath, tryNumber);

                        Directory.Delete(folderPath, true);
                    }
                    return true;
                }
                catch (IOException ex)
                {
                    logger.Error(ex);
                    logger.Error("Unable to delete folder {0}", folderPath);

                    if (ex.Message.StartsWith("The directory is not empty"))
                    {
                        tryNumber++;
                        Thread.Sleep(3000);
                    }
                    else
                    {
                        return false;
                    }
                }
            } while (tryNumber <= 3);

            return true;
        }

        public static bool DeleteFile(string filePath)
        {
            int tryNumber = 1;

            do
            {
                try
                {
                    if (File.Exists(filePath))
                    {
                        logger.Info("Deleting file {0}, try #{1}", filePath, tryNumber);

                        File.Delete(filePath);
                    }
                    return true;
                }
                catch (IOException ex)
                {
                    logger.Error(ex);
                    logger.Error("Unable to delete file {0}", filePath);

                    tryNumber++;
                    Thread.Sleep(3000);
                }
            } while (tryNumber <= 3);

            return true;
        }

        public static bool CopyFolder(string folderPathSource, string folderPathTarget)
        {
            CreateFolder(folderPathTarget);

            foreach (string file in Directory.GetFiles(folderPathSource))
            {
                string dest = Path.Combine(folderPathTarget, Path.GetFileName(file));
                try
                {
                    logger.Info("Copying file {0} to {1}", file, dest);

                    File.Copy(file, dest, true);
                }
                catch (Exception ex)
                {
                    logger.Error("Unable to copy file {0} to {1}", file, dest);
                    logger.Error(ex);

                    return false;
                }
            }

            foreach (string folder in Directory.GetDirectories(folderPathSource))
            {
                string dest = Path.Combine(folderPathTarget, Path.GetFileName(folder));
                CopyFolder(folder, dest);
            }

            return true;
        }

        public static bool CopyFile(string filePathSource, string filePathDestination)
        {
            CreateFolderForFile(filePathDestination);

            try
            {
                logger.Info("Copying file {0} to {1}", filePathSource, filePathDestination);

                File.Copy(filePathSource, filePathDestination, true);
            }
            catch (Exception ex)
            {
                logger.Error("Unable to copy file {0} to {1}", filePathSource, filePathDestination);
                logger.Error(ex);

                return false;
            }

            return true;
        }

        public static bool SaveFileToPath(string fileContents, string filePath)
        {
            return SaveFileToPath(fileContents, filePath, true);
        }

        public static bool SaveFileToPath(string fileContents, string filePath, bool writeUTF8BOM)
        {
            string folderPath = Path.GetDirectoryName(filePath);

            if (CreateFolder(folderPath) == true)
            {
                try
                {
                    logger.Info("Writing string length {0} to file {1}", fileContents.Length, filePath);

                    if (writeUTF8BOM == true)
                    {
                        File.WriteAllText(filePath, fileContents, Encoding.UTF8);
                    }
                    else
                    {
                        Encoding utf8WithoutBom = new UTF8Encoding(false);
                        File.WriteAllText(filePath, fileContents, utf8WithoutBom);
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Unable to write to file {0}", filePath);
                    logger.Error(ex);
                }
            }

            return false;
        }

        public static TextWriter SaveFileToPathWithWriter(string filePath)
        {
            string folderPath = Path.GetDirectoryName(filePath);

            if (CreateFolder(folderPath) == true)
            {
                try
                {
                    logger.Info("Opening TextWriter to file {0}", filePath);

                    return File.CreateText(filePath);
                }
                catch (Exception ex)
                {
                    logger.Error("Unable to write to file {0}", filePath);
                    logger.Error(ex);

                    return null;
                }
            }
            return null;
        }

        public static string ReadFileFromPath(string filePath)
        {
            try
            {
                if (File.Exists(filePath) == false)
                {
                    logger.Warn("Unable to find file {0}", filePath);
                }
                else
                {
                    logger.Info("Reading file {0}", filePath);
                    return File.ReadAllText(filePath, Encoding.UTF8);
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unable to read from file {0}", filePath);
                logger.Error(ex);
            }

            return String.Empty;
        }

        #endregion

        #region JSON file reading and writing

        public static JObject LoadJObjectFromFile(string jsonFilePath)
        {
            try
            {
                if (File.Exists(jsonFilePath) == false)
                {
                    logger.Warn("Unable to find file {0}", jsonFilePath);
                }
                else
                {
                    logger.Info("Reading JObject from file {0}", jsonFilePath);

                    return JObject.Parse(File.ReadAllText(jsonFilePath));
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unable to load JSON from file {0}", jsonFilePath);
                logger.Error(ex);
            }

            return null;
        }

        public static bool WriteObjectToFile(object objectToWrite, string jsonFilePath)
        {
            string folderPath = Path.GetDirectoryName(jsonFilePath);

            if (CreateFolder(folderPath) == true)
            {
                try
                {
                    logger.Info("Writing object {0} to file {1}", objectToWrite.GetType().Name, jsonFilePath);

                    using (StreamWriter sw = File.CreateText(jsonFilePath))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        serializer.NullValueHandling = NullValueHandling.Include;
                        serializer.Formatting = Newtonsoft.Json.Formatting.Indented;
                        serializer.Serialize(sw, objectToWrite);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Unable to write object to file {0}", jsonFilePath);
                    logger.Error(ex);
                }
            }

            return false;
        }

        public static JArray LoadJArrayFromFile(string jsonFilePath)
        {
            try
            {
                if (File.Exists(jsonFilePath) == false)
                {
                    logger.Warn("Unable to find file {0}", jsonFilePath);
                }
                else
                {
                    logger.Info("Reading JArray from file {0}", jsonFilePath);

                    return JArray.Parse(File.ReadAllText(jsonFilePath));
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unable to load JSON from file {0}", jsonFilePath);
                logger.Error(ex);
            }

            return null;
        }

        public static bool WriteJArrayToFile(JArray array, string jsonFilePath)
        {
            logger.Info("Writing JSON Array with {0} elements to file {1}", array.Count, jsonFilePath);

            return WriteObjectToFile(array, jsonFilePath);
        }

        public static List<T> LoadListOfObjectsFromFile<T>(string jsonFilePath)
        {
            try
            {
                if (File.Exists(jsonFilePath) == false)
                {
                    logger.Warn("Unable to find file {0}", jsonFilePath);
                }
                else
                {
                    logger.Info("Reading List<{0}> from file {1}", typeof(T), jsonFilePath);

                    return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(jsonFilePath));
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unable to load JSON from file {0}", jsonFilePath);
                logger.Error(ex);
            }

            return null;
        }

        #endregion

        #region CSV reading and writing

        public static bool WriteListToCSVFile<T>(List<T> listToWrite, ClassMap<T> classMap, string csvFilePath)
        {
            return WriteListToCSVFile(listToWrite, classMap, csvFilePath, false);
        }

        public static bool WriteListToCSVFile<T>(List<T> listToWrite, ClassMap<T> classMap, string csvFilePath, bool appendToExistingFile)
        {
            return WriteListToCSVFile(listToWrite, classMap, csvFilePath, appendToExistingFile, true);

        }
        
        public static bool WriteListToCSVFile<T>(List<T> listToWrite, ClassMap<T> classMap, string csvFilePath, bool appendToExistingFile, bool includeHeader)
        {
            if (listToWrite == null) return true;

            string folderPath = Path.GetDirectoryName(csvFilePath);

            if (CreateFolder(folderPath) == true)
            {
                try
                {
                    logger.Trace("Writing list of type {0} with {1} elements to file {2}, append mode {3}", typeof(T), listToWrite.Count, csvFilePath, appendToExistingFile);

                    if (appendToExistingFile == true && File.Exists(csvFilePath) == true)
                    {
                        // Append without header
                        using (StreamWriter sw = File.AppendText(csvFilePath))
                        {
                            CsvWriter csvWriter = new CsvWriter(sw, CultureInfo.InvariantCulture);
                            csvWriter.Configuration.RegisterClassMap(classMap);
                            csvWriter.Configuration.HasHeaderRecord = false;
                            csvWriter.Configuration.NewLine = NewLine.LF;
                            csvWriter.WriteRecords(listToWrite);
                        }
                    }
                    else
                    {
                        // Create new with header
                        using (StreamWriter sw = File.CreateText(csvFilePath))
                        {
                            CsvWriter csvWriter = new CsvWriter(sw, CultureInfo.InvariantCulture);
                            csvWriter.Configuration.RegisterClassMap(classMap);
                            csvWriter.Configuration.HasHeaderRecord = includeHeader;
                            csvWriter.Configuration.NewLine = NewLine.LF;
                            csvWriter.WriteRecords(listToWrite);
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Unable to write CSV to file {0}", csvFilePath);
                    logger.Error(ex);
                }
            }

            return false;
        }

        public static MemoryStream WriteListToMemoryStream<T>(List<T> listToWrite, ClassMap<T> classMap)
        {
            try
            {
                if (listToWrite == null) return null;

                logger.Trace("Writing list with {0} elements containing type {1} to memory stream", listToWrite.Count, typeof(T));

                MemoryStream ms = new MemoryStream(1024 * listToWrite.Count);
                StreamWriter sw = new StreamWriter(ms);
                CsvWriter csvWriter = new CsvWriter(sw, CultureInfo.InvariantCulture);
                csvWriter.Configuration.RegisterClassMap(classMap);
                csvWriter.Configuration.HasHeaderRecord = true;
                csvWriter.WriteRecords(listToWrite);

                sw.Flush();

                // Rewind the stream
                ms.Position = 0;

                return ms;
            }
            catch (Exception ex)
            {
                logger.Error("Unable to write CSV to memory stream");
                logger.Error(ex);
            }

            return null;
        }

        public static List<T> ReadListFromCSVFile<T>(string csvFilePath, ClassMap<T> classMap)
        {
            return ReadListFromCSVFile<T>(csvFilePath, classMap, String.Empty);
        }

        public static List<T> ReadListFromCSVFile<T>(string csvFilePath, ClassMap<T> classMap, string skipRecordPrefix)
        {
            try
            {
                logger.Trace("Reading list of type {0} from file {1}", typeof(T), csvFilePath);

                if (File.Exists(csvFilePath) == false)
                {
                    logger.Warn("File {0} does not exist", csvFilePath);
                }
                else
                {
                    using (StreamReader sr = File.OpenText(csvFilePath))
                    {
                        CsvReader csvReader = new CsvReader(sr, CultureInfo.InvariantCulture);
                        csvReader.Configuration.RegisterClassMap(classMap);
                        csvReader.Configuration.BadDataFound = rc =>
                        {
                            logger.Warn("Bad thing on row {0}, char {1}, field '{2}'", rc.Row, rc.CharPosition, rc.Field);
                            logger.Warn(rc.RawRecord);
                        };
                        if (skipRecordPrefix.Length > 0)
                        {
                            csvReader.Configuration.ShouldSkipRecord = record => record.FirstOrDefault()?.StartsWith(skipRecordPrefix) ?? false;
                        }
                        return csvReader.GetRecords<T>().ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Unable to read CSV from file {0}", csvFilePath);
                logger.Error(ex);
            }

            return null;
        }

        public static bool AppendTwoCSVFiles(string csvToAppendToFilePath, string csvFromWhichToAppendFilePath)
        {
            string folderPath = Path.GetDirectoryName(csvToAppendToFilePath);

            if (CreateFolder(folderPath) == true)
            {
                try
                {
                    logger.Trace("Adding to to CSV file {0} this CSV file {1}", csvToAppendToFilePath, csvFromWhichToAppendFilePath);

                    if (File.Exists(csvFromWhichToAppendFilePath) == true)
                    {
                        if (File.Exists(csvToAppendToFilePath) == true)
                        {
                            // Append without header
                            using (FileStream sr = File.Open(csvFromWhichToAppendFilePath, FileMode.Open))
                            {
                                while (true)
                                {
                                    if (sr.Position == sr.Length) break;

                                    char c = (char)sr.ReadByte();
                                    if (c == '\n' || c == '\r')
                                    {
                                        break;
                                    }
                                }

                                using (FileStream csvToAppendToSW = File.Open(csvToAppendToFilePath, FileMode.Append))
                                {
                                    copyStream(sr, csvToAppendToSW);
                                }
                            }
                        }
                        else
                        {
                            // Create new file with header
                            using (StreamReader sr = File.OpenText(csvFromWhichToAppendFilePath))
                            {
                                using (StreamWriter sw = File.CreateText(csvToAppendToFilePath))
                                {
                                    copyStream(sr.BaseStream, sw.BaseStream);
                                }
                            }
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    logger.Error("Appending file {0} and file {1} failed", csvToAppendToFilePath, csvFromWhichToAppendFilePath);
                    logger.Error(ex);
                }
            }

            return false;
        }

        public static bool AppendTwoCSVFiles(FileStream csvToAppendToSW, string csvToAppendFilePath)
        {
            try
            {
                logger.Trace("Appending CSV file {0} to another CSV file open as stream", csvToAppendFilePath);

                if (File.Exists(csvToAppendFilePath) == true)
                {
                    using (FileStream sr = File.Open(csvToAppendFilePath, FileMode.Open))
                    {
                        // If the stream to append to is already ahead, that means we don't need headers anymore
                        if (csvToAppendToSW.Position > 0)
                        {
                            // Go through the first line to remove the header
                            while (true)
                            {
                                if (sr.Position == sr.Length) break;

                                char c = (char)sr.ReadByte();
                                if (c == '\n' || c == '\r')
                                {
                                    // Found the end of the first lne
                                    break;
                                }
                            }
                        }

                        copyStream(sr, csvToAppendToSW);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Appending CSV file {0} to another CSV file open as stream", csvToAppendFilePath);
                logger.Error(ex);
            }

            return false;
        }

        private static void copyStream(Stream input, Stream output)
        {
            // 1048576 = 1024*1024 = 2^20 = 1MB
            byte[] buffer = new byte[1048576];
            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        #endregion

        #region File name handling

        public static string GetFileSystemSafeString(string fileOrFolderNameToClear)
        {
            if (fileOrFolderNameToClear == null) fileOrFolderNameToClear = String.Empty;

            foreach (var c in Path.GetInvalidFileNameChars())
            {
                fileOrFolderNameToClear = fileOrFolderNameToClear.Replace(c, '-');
            }

            return fileOrFolderNameToClear;
        }

        public static string GetShortenedEntityNameForFileSystem(string entityName, int maxLength)
        {
            string originalEntityName = entityName;

            // First, strip out unsafe characters
            entityName = GetFileSystemSafeString(entityName);

            // Second, shorten the string 
            if (entityName.Length > maxLength) entityName = entityName.Substring(0, maxLength);

            return entityName;
        }

        #endregion
    }
}
