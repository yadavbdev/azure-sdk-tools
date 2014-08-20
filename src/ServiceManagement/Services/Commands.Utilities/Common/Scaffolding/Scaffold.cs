// ----------------------------------------------------------------------------------
//
// Copyright Microsoft Corporation
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// ----------------------------------------------------------------------------------

using Microsoft.WindowsAzure.Commands.Common;
using Microsoft.WindowsAzure.Commands.Common.Properties;
 using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.Commands.Utilities.CloudService.Scaffolding
{
    public class Scaffold
    {
        public List<ScaffoldFile> Files { get; private set; }

        public Scaffold()
        {
            Files = new List<ScaffoldFile>();
        }

        public static void ReplaceParameterRule(string path, Dictionary<string, object> parameters)
        {
            string contents = ProfileClient.DataStore.ReadFileAsText(path);

            contents = ReplaceParameter(contents, parameters);

            ProfileClient.DataStore.WriteFile(path, contents);
        }

        public static string ReplaceParameter(string text, Dictionary<string, object> parameters)
        {
            foreach (KeyValuePair<string, object> pair in parameters)
            {
                text = text.Replace(string.Format("${0}$", pair.Key), pair.Value.ToString());
            }
            return text;
        }

        public static void GenerateScaffolding(string sourcePath, string destPath, Dictionary<string, object> parameters)
        {
            sourcePath = Path.GetFullPath(sourcePath);
            Scaffold scaffold = Parse(Path.Combine(sourcePath, Resources.ScaffoldXml));

            foreach (ScaffoldFile file in scaffold.Files)
            {
                string sourceDirectory; 
                if(string.IsNullOrEmpty(file.SourceDirectory) ||
                    !parameters.ContainsKey(file.SourceDirectory))
                {
                    sourceDirectory = sourcePath;
                } else {
                    sourceDirectory = (string) parameters[file.SourceDirectory];
                }


                foreach (string path in GetPaths(sourceDirectory, file))
                {
                    string tempPath;

                    if (string.IsNullOrEmpty(file.PathExpression))
                    {
                        tempPath = !string.IsNullOrEmpty(file.TargetPath) ? file.TargetPath : GetRelativePath(sourceDirectory, path);
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(file.TargetPath))
                        {
                            tempPath = GetRelativePath(sourceDirectory, path);
                        }
                        else
                        {
                            tempPath = GetRelativePath(sourceDirectory, path);
                            string t1 = tempPath.Substring(tempPath.IndexOf('\\') + 1);
                            tempPath = Path.Combine(file.TargetPath, t1);
                        }
                    }

                    tempPath = Path.Combine(destPath, tempPath);
                    ProfileClient.DataStore.CreateDirectory(Path.GetDirectoryName(tempPath));

                    ProfileClient.DataStore.CopyFile(path, tempPath);

                    foreach (ScaffoldRule rule in file.Rules)
                    {
                        rule(tempPath, parameters);
                    }

                    if (!file.Copy)
                    {
                        ProfileClient.DataStore.DeleteFile(tempPath);
                    }
                }
            }
        }

        private static List<string> GetPaths(string sourcePath, ScaffoldFile file)
        {
            List<string> paths = new List<string>();
            if (!string.IsNullOrEmpty(file.PathExpression))
            {
                foreach (string fullPath in ProfileClient.DataStore.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
                {
                    string relativePath = GetRelativePath(sourcePath, fullPath);
                    if (Regex.IsMatch(relativePath, file.PathExpression))
                    {
                        paths.Add(fullPath);
                    }
                }
            }
            else
            {
                paths.Add(Path.Combine(sourcePath, file.Path));
            }

            return paths;
        }

        internal static string GetRelativePath(string sourcePath, string fullPath)
        {
            string relativePath = fullPath.Substring(sourcePath.Length, fullPath.Length - sourcePath.Length).TrimStart('\\');
            return relativePath;
        }

        public static Scaffold Parse(string path)
        {
            XDocument document = XDocument.Parse(ProfileClient.DataStore.ReadFileAsText(path));
            Scaffold scaffold = new Scaffold();

            foreach (XElement fileElement in document.Root.Elements(XName.Get("ScaffoldFile")))
            {
                ScaffoldFile scaffoldFile = new ScaffoldFile();
                scaffoldFile.Path = GetAttribute(fileElement, "Path");
                scaffoldFile.Copy = bool.Parse(GetAttribute(fileElement, "Copy") ?? "true");
                scaffoldFile.TargetPath = GetAttribute(fileElement, "TargetPath");
                scaffoldFile.PathExpression = GetAttribute(fileElement, "PathExpression");
                scaffoldFile.SourceDirectory = GetAttribute(fileElement, "SourceDirectory");

                foreach (XElement ruleElement in fileElement.Elements())
                {
                    string[] names = ruleElement.Name.LocalName.Split('.');
                    string className = string.Format("{0}.{1}", typeof(Scaffold).Namespace, names[0]);
                    string methodName = names[1];
                    Type type = typeof(Scaffold).Assembly.GetType(className);
                    MethodInfo ruleInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.InvokeMethod | BindingFlags.Public);
                    ScaffoldRule rule = Delegate.CreateDelegate(typeof(ScaffoldRule), ruleInfo) as ScaffoldRule;
                    scaffoldFile.Rules.Add(rule);
                }

                scaffold.Files.Add(scaffoldFile);
            }

            return scaffold;
        }

        private static string GetAttribute(XElement fileElement, string attributeName)
        {
            string value = null;
            XAttribute attribute = fileElement.Attribute(XName.Get(attributeName));
            if (attribute != null)
            {
                value = attribute.Value;
            }

            return value;
        }
    }

    public delegate void ScaffoldRule(string path, Dictionary<string, object> parameters);
}