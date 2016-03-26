﻿/* ===================================================================
 License:
    DiscerningEye - FFXIV Gathering Companion App
    SettingsViewModel.cs


    Copyright(C) 2015 - 2016  Christopher Whitley

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.If not, see<http://www.gnu.org/licenses/> .
  =================================================================== */



using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using NAudio.Wave;
using Prism.Commands;

namespace DiscerningEye.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
              
        
        private IWavePlayer _waveOutDevice;
        private AudioFileReader _audioFileReader;

        private string _uiAccentSelectedValue;
        /// <summary>
        /// Gets or sets the selected UI Accent Value
        /// </summary>
        /// <remarks>
        /// This is bound to a Combobox.SelectedValue on the SettingsView
        /// </remarks>
        public string UIAccentSelectedValue
        {
            get { return this._uiAccentSelectedValue; }
            set { SetProperty(ref _uiAccentSelectedValue, value); }
        }

        private string _uiAppThemeSelectedValue;
        /// <summary>
        /// Gets or sets the selected UI theme
        /// </summary>
        /// <remarks>
        /// This is bound to a Combobox.SelectedValue on the SettingsView
        /// </remarks>
        public string UIAppThemeSelectedValue
        {
            get { return this._uiAppThemeSelectedValue; }
            set { SetProperty(ref this._uiAppThemeSelectedValue, value); }
        }

        private string _testButtonText;
        /// <summary>
        /// Gets or sets a string representing the text shown for the test button
        /// </summary>
        /// <remarks>
        /// This is bound to the content property of a button on Settings.xaml
        /// </remarks>
        public string TestButtonText
        {
            get { return this._testButtonText; }
            set { SetProperty(ref this._testButtonText, value); }
        }

        //=========================================================
        //  Commands
        //=========================================================
        /// <summary>
        /// Gets or sets the SelectFileCommand
        /// </summary>
        /// <remarks>
        /// This is bound to a control on Settings.xaml
        /// </remarks>
        public DelegateCommand SelectFileCommand { get; private set; }

        /// <summary>
        /// Gets or sets the TestNotificationCommand
        /// </summary>
        /// <remarks>
        /// This is bound to a control on Settings.xaml
        /// </remarks>
        public DelegateCommand TestNotificationCommand { get; private set; }


        //=========================================================
        //  Constructor
        //=========================================================
        /// <summary>
        /// Initilizes a new instance of the SettingViewModel class
        /// </summary>
        public SettingsViewModel()
        {
            //  Setup the commands
            this.SelectFileCommand = new DelegateCommand(() =>
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Filter = "MP3 (*.mp3)|*.mp3";
                if ((bool)ofd.ShowDialog())
                {
                    Properties.Settings.Default.NotificationToneUri = ofd.FileName;
                }
            }, () => true);


            this.TestNotificationCommand = new DelegateCommand(async() =>
            {
                if (string.IsNullOrWhiteSpace(Properties.Settings.Default.NotificationToneUri))
                {
                    MetroDialogSettings settings = new MetroDialogSettings();
                    settings.AffirmativeButtonText = "Ok";
                    settings.AnimateHide = true;
                    settings.AnimateShow = true;
                    settings.DefaultButtonFocus = MessageDialogResult.Affirmative;
                    await Views.MainWindow.View.ShowMessageAsync("Notification Path ", string.Format("Notification file path cannot be empty to test sound"), MessageDialogStyle.Affirmative, settings);
                    return;
                }

                if (_waveOutDevice.PlaybackState != PlaybackState.Playing)
                {

                    _audioFileReader = new AudioFileReader(Properties.Settings.Default.NotificationToneUri);
                    _audioFileReader.Volume = (float)Properties.Settings.Default.NotificationToneVolume / 100.0f;
                    _waveOutDevice.Init(_audioFileReader);
                    _waveOutDevice.Play();
                    this.TestButtonText = "Cancel Test";
                }
                else
                {
                    _waveOutDevice.Stop();
                }
            }, () => true);


            //  Set default values for propeties
            this.TestButtonText = "Test Sound";
            this.UIAccentSelectedValue = Properties.Settings.Default.UIAccent;
            this.UIAppThemeSelectedValue = Properties.Settings.Default.UIAppTheme;

            //  Initilize the wave out device
            _waveOutDevice = new WaveOut();
            _waveOutDevice.PlaybackStopped += _waveOutDevice_PlaybackStopped;
        }


        //=========================================================
        //  Events
        //=========================================================
        /// <summary>
        /// Occurs when the _waveOutDevice sound playback is stopped.
        /// </summary>
        /// <remarks>
        /// This is used to ensure that the object is disposed after playback has stopped
        /// to prevent any type of memory leak and to also allow users to test sounds
        /// again if they choose to test sound immediatly after stopping playback
        /// </remarks>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _waveOutDevice_PlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
            this.TestButtonText = "Test Sound";
        }


        //=========================================================
        //  Interface Implementations
        //=========================================================
        #region IDisposeable Implementation

        protected override void OnDispose()
        {

            if (_waveOutDevice != null)
            {
                _waveOutDevice.PlaybackStopped -= _waveOutDevice_PlaybackStopped;
                _waveOutDevice.Stop();

            }
            if (_audioFileReader != null)
            {
                _audioFileReader.Dispose();
                _audioFileReader = null;
            }
            if (_waveOutDevice != null)
            {
                _waveOutDevice.Dispose();
                _waveOutDevice = null;
            }


            this.UIAccentSelectedValue = null;
            this.UIAppThemeSelectedValue = null;
            this.SelectFileCommand = null;
            this.TestNotificationCommand = null;
        }

        #endregion IDisposeable Implementation
    }
}
