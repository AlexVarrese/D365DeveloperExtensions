﻿using CrmDeveloperExtensions2.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using NpmHistory = CrmDeveloperExtensions2.Core.Models.NpmHistory;

namespace TemplateWizards
{
    public partial class NpmPicker
    {
        private readonly NpmHistory _npmHistory;
        public NpmPackage SelectedPackage { get; set; }

        public NpmPicker(NpmHistory history)
        {
            InitializeComponent();

            _npmHistory = history;

            GetPackage(history);
        }

        private void GetPackage(NpmHistory history)
        {
            Versions.Items.Clear();

            if (LimitVersions.IsChecked != null && LimitVersions.IsChecked.Value)
                history = FilterLatestVersions(history);

            List<string> versions = history.versions.OrderByDescending(s => s).ToList();

            VersionsGrid.Columns[0].Header = history.name;

            foreach (string version in versions)
            {
                ListViewItem item = new ListViewItem
                {
                    Content = version,
                    Tag = new NpmPackage
                    {
                        Name = history.name,
                        Version = version
                    }
                };

                Versions.Items.Add(item);
            }

            Versions.SelectedIndex = 0;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            CloseDialog(false);
        }

        private void Ok_OnClick(object sender, RoutedEventArgs e)
        {
            CloseDialog(true);
        }

        private void CloseDialog(bool result)
        {
            DialogResult = result;
            Close();
        }

        private void Versions_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListView versions = (ListView)sender;
            ListBoxItem item = versions.SelectedItem as ListViewItem;
            if (item == null)
                return;

            SelectedPackage = (NpmPackage)item.Tag;
        }

        private static NpmHistory FilterLatestVersions(NpmHistory history)
        {
            NpmHistory filteredHistory = new NpmHistory
            {
                name = history.name,
                description = history.description,
                versions = new List<string>()
            };

            Version firstVersion = CrmDeveloperExtensions2.Core.Versioning.StringToVersion(history.versions[0]);
            var currentMajor = firstVersion.Major;
            var currentMinor = firstVersion.Minor;
            var currentBuild = firstVersion.Build;
            var currentVersion = history.versions[0];

            for (int i = 0; i < history.versions.Count; i++)
            {
                if (i == history.versions.Count - 1)
                    filteredHistory.versions.Add(history.versions[i]);

                Version ver = CrmDeveloperExtensions2.Core.Versioning.StringToVersion(history.versions[i]);

                if (ver.Major > currentMajor)
                {
                    currentMajor = ver.Major;
                    currentMinor = ver.Minor;
                    currentBuild = ver.Build;
                    filteredHistory.versions.Add(currentVersion);
                    currentVersion = history.versions[i];
                    continue;
                }

                if (ver.Minor > currentMinor)
                {
                    currentMinor = ver.Minor;
                    currentBuild = ver.Build;
                    filteredHistory.versions.Add(currentVersion);
                    currentVersion = history.versions[i];
                    continue;
                }

                if (ver.Build > currentBuild)
                    currentVersion = history.versions[i];
            }

            return filteredHistory;
        }

        private void LimitVersions_Checked(object sender, RoutedEventArgs e)
        {
            if (_npmHistory != null)
                GetPackage(_npmHistory);
        }
    }
}