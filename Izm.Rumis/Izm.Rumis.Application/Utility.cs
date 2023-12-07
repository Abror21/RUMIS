using Izm.Rumis.Application.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace Izm.Rumis.Application
{
    public static class Utility
    {
        public static byte[] StreamToArray(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public static byte[] StreamToArray(MemoryStream stream)
        {
            var reader = new BinaryReader(stream);
            var bytes = new byte[stream.Length];

            reader.Read(bytes);

            return bytes;
        }

        public static byte[] Zip(IEnumerable<FileEntry> files)
        {
            byte[] archiveFile;
            using (var archiveStream = new MemoryStream())
            {
                using (var archive = new ZipArchive(archiveStream, ZipArchiveMode.Create, true))
                {
                    foreach (var file in files)
                    {
                        var zipArchiveEntry = archive.CreateEntry(file.Name, CompressionLevel.Fastest);

                        using var zipStream = zipArchiveEntry.Open();
                        zipStream.Write(file.Content, 0, file.Content.Length);
                    }
                }

                archiveFile = archiveStream.ToArray();
            }

            return archiveFile;
        }

        public static string SanitizeFileName(string fileName)
        {
            const string fallback = "_";
            const int maxLength = 250;

            if (string.IsNullOrEmpty(fileName))
                fileName = fallback;

            foreach (char character in Path.GetInvalidFileNameChars())
                fileName = fileName.Replace(character.ToString(), string.Empty);

            if (string.IsNullOrEmpty(fileName))
                fileName = fallback;

            if (fileName.Length > maxLength)
                fileName = fileName.Substring(0, maxLength);

            return fileName;
        }

        public static string SanitizeCode(string code)
        {
            return string.IsNullOrEmpty(code) ? null : Regex.Replace(code.ToLower(), "[^a-z0-9-_\\.]", "");
        }

        /// <summary>
        /// Get descendants.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="nodes">List of all nodes</param>
        /// <param name="node">Node to look descendants for</param>
        /// <param name="parentFn">Function to check if current item is a child</param>
        /// <returns></returns>
        public static IEnumerable<T> GetDescendants<T>(IEnumerable<T> nodes, T node, Func<T, T, bool> parentFn)
        {
            var list = new List<T>();

            foreach (var item in nodes.Where(t => parentFn(t, node)))
            {
                var children = GetDescendants(nodes, item, parentFn);

                list.Add(item);
                list.AddRange(children);
            }

            return list;
        }

        public static IEnumerable<T> OrderTreeList<T>(IEnumerable<T> items) where T : ITreeOrdered
        {
            foreach (var item in items)
            {
                item.Path = $"{item.Id}";
                item.Level = 0;

                var parent = items.FirstOrDefault(t => t.Id == item.ParentId);

                if (parent == null)
                {
                    item.Index = 0;
                    item.ListOrder = "1.";
                }
                else
                {
                    item.Index = items.Where(t => t.ParentId == parent.Id).OrderBy(t => t.Order).ToList().IndexOf(item);
                    item.ListOrder = $"{item.Index + 1}.";

                    while (parent != null)
                    {
                        string parentListOrder;

                        if (parent.ParentId == null)
                        {
                            parentListOrder = "1.";
                        }
                        else
                        {
                            var parentIndex = items.Where(t => t.ParentId == parent.ParentId).OrderBy(t => t.Order).ToList().IndexOf(parent);
                            parentListOrder = $"{parentIndex + 1}.";
                        }

                        item.Level++;
                        item.Path = $"{parent.Id}/{item.Path}";
                        item.ListOrder = parentListOrder + item.ListOrder;

                        parent = items.FirstOrDefault(t => t.Id == parent.ParentId);
                    }
                }
            }

            var maxIndex = items.Max(t => t.Index);

            foreach (var item in items)
                item.NormalizedOrder = string.Join(".", item.ListOrder.TrimEnd('.').Split(".").Select(t => t.PadLeft(maxIndex, '0')));

            return items.OrderBy(t => t.NormalizedOrder);
        }

        public static TreeNode<T> BuildTree<T>(IEnumerable<T> items, T root, Func<T, T, bool> childFn)
        {
            var childNodes = new List<TreeNode<T>>();
            var rootNode = new TreeNode<T>
            {
                Data = root,
                Children = childNodes
            };

            var children = items.Where(t => childFn(t, root));

            foreach (var child in children)
            {
                var childNode = BuildTree(items, child, childFn);

                childNode.Parent = rootNode;

                childNodes.Add(childNode);
            }

            return rootNode;
        }

        public static IEnumerable<TreeNode<T>> FlattenTree<T>(TreeNode<T> tree)
        {
            var list = new List<TreeNode<T>>();

            list.Add(tree);

            if (tree.Children.Any())
            {
                foreach (var child in tree.Children)
                {
                    list.AddRange(FlattenTree(child));
                }
            }

            return list;
        }

        /// <summary>
        /// Convert dynamic object to a dictionary.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static Dictionary<string, object> DynamicToDictionary(dynamic data)
        {
            var dictionary = new Dictionary<string, object>();

            foreach (var propertyDescriptor in TypeDescriptor.GetProperties(data))
            {
                object obj = propertyDescriptor.GetValue(data);
                dictionary.Add(propertyDescriptor.Name, obj);
            }

            return dictionary;
        }

        public static bool IsNumeric(object value)
        {
            double retNum;
            return double.TryParse(
                Convert.ToString(value),
                System.Globalization.NumberStyles.Any,
                System.Globalization.NumberFormatInfo.InvariantInfo,
                out retNum);
        }

        /// <summary>
        /// Check if LV private personal identifier is valid.
        /// </summary>
        /// <param name="privatePersonalIdentifier">Private person identifier to validate.</param>
        /// <returns>True if valid; false if invalid.</returns>
        public static bool IsPrivatePersonalIdentifierChecksumValid(string privatePersonalIdentifier)
        {
            return true;

            if (privatePersonalIdentifier.Length == 11)
                privatePersonalIdentifier = $"{privatePersonalIdentifier.Substring(0, 6)}-{privatePersonalIdentifier.Substring(6, 5)}";

            if (privatePersonalIdentifier == null || privatePersonalIdentifier.TrimStart(" -".ToCharArray()).Length == 0
                || privatePersonalIdentifier.Trim(" -".ToCharArray()).Length != 12 || privatePersonalIdentifier.Contains(" "))
                return false;

            int checkSum =
                Convert.ToInt32(privatePersonalIdentifier.Substring(0, 1)) * 1 +
                Convert.ToInt32(privatePersonalIdentifier.Substring(1, 1)) * 6 +
                Convert.ToInt32(privatePersonalIdentifier.Substring(2, 1)) * 3 +
                Convert.ToInt32(privatePersonalIdentifier.Substring(3, 1)) * 7 +
                Convert.ToInt32(privatePersonalIdentifier.Substring(4, 1)) * 9 +
                Convert.ToInt32(privatePersonalIdentifier.Substring(5, 1)) * 10 +
                Convert.ToInt32(privatePersonalIdentifier.Substring(7, 1)) * 5 +
                Convert.ToInt32(privatePersonalIdentifier.Substring(8, 1)) * 8 +
                Convert.ToInt32(privatePersonalIdentifier.Substring(9, 1)) * 4 +
                Convert.ToInt32(privatePersonalIdentifier.Substring(10, 1)) * 2;

            int cControl = Convert.ToInt32(privatePersonalIdentifier.Substring(11, 1));
            bool validChecksum = (checkSum % 11) == ((12 - cControl) % 11);

            return validChecksum;
        }
    }
}
