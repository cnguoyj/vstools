﻿/****************************************************************************
**
** Copyright (C) 2020 The Qt Company Ltd.
** Contact: https://www.qt.io/licensing/
**
** This file is part of the Qt VS Tools.
**
** $QT_BEGIN_LICENSE:GPL-EXCEPT$
** Commercial License Usage
** Licensees holding valid commercial Qt licenses may use this file in
** accordance with the commercial license agreement provided with the
** Software or, alternatively, in accordance with the terms contained in
** a written agreement between you and The Qt Company. For licensing terms
** and conditions see https://www.qt.io/terms-conditions. For further
** information use the contact form at https://www.qt.io/contact-us.
**
** GNU General Public License Usage
** Alternatively, this file may be used under the terms of the GNU
** General Public License version 3 as published by the Free Software
** Foundation with exceptions as appearing in the file LICENSE.GPL3-EXCEPT
** included in the packaging of this file. Please review the following
** information to ensure the GNU General Public License requirements will
** be met: https://www.gnu.org/licenses/gpl-3.0.html.
**
** $QT_END_LICENSE$
**
****************************************************************************/

using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Win32;
using Microsoft.VisualStudio.Shell;
using QtVsTools.Core;
using QtVsTools.Common;

namespace QtVsTools.Options
{
    using static EnumExt;

    public class QtOptionPage : DialogPage
    {
        public enum QtOptions
        {
            [String("QMLDebug_Timeout")] QmlDebugTimeout
        }

        public enum Timeout : uint { Disabled = 0 }

        class TimeoutConverter : EnumConverter
        {
            public TimeoutConverter(Type t) : base(t)
            { }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext c)
                => true;

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext c)
                => false;

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext c)
                => new StandardValuesCollection(new[] { Timeout.Disabled });

            public override object ConvertFrom(
                ITypeDescriptorContext context,
                CultureInfo culture,
                object value)
            {
                uint n = 0;
                try {
                    n = Convert.ToUInt32(value);
                } catch { }
                return (Timeout)n;
            }

            public override object ConvertTo(
                ITypeDescriptorContext context,
                CultureInfo culture,
                object value,
                Type destinationType)
            {
                if (destinationType == typeof(string))
                    return value.ToString();
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }

        [Category("Source control")]
        [DisplayName("Ask before checking out files")]
        public bool CheckoutPrompt { get; set; }

        [Category("Source control")]
        [DisplayName("Enable file check-out")]
        public bool Checkout { get; set; }

        [Category("Linguist (lupdate/lrelease)")]
        [DisplayName("Default lrelease options")]
        public string DefaultLReleaseOptions { get; set; }

        [Category("Linguist (lupdate/lrelease)")]
        [DisplayName("Default lupdate options")]
        public string DefaultLUpdateOptions { get; set; }

        [Category("Linguist (lupdate/lrelease)")]
        [DisplayName("Run lupdate during build")]
        public bool EnableLUpdateOnBuild { get; set; }

        [Category("Meta-Object Compiler (moc)")]
        [DisplayName("Default moc generated files directory")]
        public string DefaultMocDir { get; set; }

        [Category("Meta-Object Compiler (moc)")]
        [DisplayName("Default additional moc options ")]
        public string AdditionalMocOptions { get; set; }

        [Category("Meta-Object Compiler (moc)")]
        [DisplayName("Enable automatic moc")]
        public bool AutoMoc { get; set; }

        [Category("Resource Compiler (rcc)")]
        [DisplayName("Default rcc generated files directory")]
        public string DefaultRccDir { get; set; }

        [Category("User Interface Compiler (uic)")]
        [DisplayName("Default uic generated files directory")]
        public string DefaultUicDir { get; set; }

        [Category("QML Debugging")]
        [DisplayName("Runtime connection timeout (msecs)")]
        [TypeConverter(typeof(TimeoutConverter))]
        public Timeout QmlDebuggerTimeout { get; set; }

        public override void ResetSettings()
        {
            CheckoutPrompt = true;
            Checkout = true;
            DefaultLReleaseOptions = "";
            DefaultLUpdateOptions = "";
            EnableLUpdateOnBuild = false;
            DefaultMocDir = "";
            AdditionalMocOptions = "";
            AutoMoc = true;
            DefaultRccDir = "";
            DefaultUicDir = "";
            QmlDebuggerTimeout = (Timeout)60000;
        }

        public override void LoadSettingsFromStorage()
        {
            ResetSettings();
            try {
                using (var key = Registry.CurrentUser
                    .OpenSubKey(@"SOFTWARE\" + Resources.registryPackagePath, writable: false)) {
                    if (key == null)
                        return;
                    if (key.GetValue(Resources.askBeforeCheckoutFileKeyword) is int checkoutPrompt)
                        CheckoutPrompt = (checkoutPrompt != 0);
                    if (key.GetValue(Resources.disableCheckoutFilesKeyword) is int disableCheckout)
                        Checkout = (disableCheckout == 0);
                    if (key.GetValue(Resources.lreleaseOptionsKeyword) is string lreleaseOptions)
                        DefaultLReleaseOptions = lreleaseOptions;
                    if (key.GetValue(Resources.lupdateOptionsKeyword) is string lupdateOptions)
                        DefaultLUpdateOptions = lupdateOptions;
                    if (key.GetValue(Resources.lupdateKeyword) is int lupdateOnBuild)
                        EnableLUpdateOnBuild = (lupdateOnBuild != 0);
                    if (key.GetValue(Resources.mocDirKeyword) is string mocDir)
                        DefaultMocDir = mocDir;
                    if (key.GetValue(Resources.mocOptionsKeyword) is string mocOptions)
                        AdditionalMocOptions = mocOptions;
                    if (key.GetValue(Resources.disableAutoMocStepsUpdateKeyword) is int autoMocOff)
                        AutoMoc = (autoMocOff == 0);
                    if (key.GetValue(Resources.rccDirKeyword) is string rccDir)
                        DefaultRccDir = rccDir;
                    if (key.GetValue(Resources.uicDirKeyword) is string uicDir)
                        DefaultUicDir = uicDir;
                    if (key.GetValue(QtOptions.QmlDebugTimeout.Cast<string>()) is int qmlTimeout)
                        QmlDebuggerTimeout = (Timeout)qmlTimeout;
                }
            } catch (Exception exception) {
                Messages.PaneMessageSafe(Vsix.Instance.Dte,
                    exception.Message + "\r\n\r\nStacktrace:\r\n" + exception.StackTrace, 5000);
            }
        }

        public override void SaveSettingsToStorage()
        {
            try {
                using (var key = Registry.CurrentUser
                    .CreateSubKey(@"SOFTWARE\" + Resources.registryPackagePath)) {
                    if (key == null)
                        return;
                    key.SetValue(Resources.askBeforeCheckoutFileKeyword, CheckoutPrompt ? 1 : 0);
                    key.SetValue(Resources.disableCheckoutFilesKeyword, Checkout ? 0 : 1);
                    key.SetValue(Resources.lreleaseOptionsKeyword, DefaultLReleaseOptions);
                    key.SetValue(Resources.lupdateOptionsKeyword, DefaultLUpdateOptions);
                    key.SetValue(Resources.lupdateKeyword, EnableLUpdateOnBuild ? 1 : 0);
                    key.SetValue(Resources.mocDirKeyword, DefaultMocDir);
                    key.SetValue(Resources.mocOptionsKeyword, AdditionalMocOptions);
                    key.SetValue(Resources.disableAutoMocStepsUpdateKeyword, AutoMoc ? 0 : 1);
                    key.SetValue(Resources.rccDirKeyword, DefaultRccDir);
                    key.SetValue(Resources.uicDirKeyword, DefaultUicDir);
                    key.SetValue(
                        QtOptions.QmlDebugTimeout.Cast<string>(),
                        (int)QmlDebuggerTimeout);
                }
            } catch (Exception exception) {
                Messages.PaneMessageSafe(Vsix.Instance.Dte,
                    exception.Message + "\r\n\r\nStacktrace:\r\n" + exception.StackTrace, 5000);
            }
        }
    }
}
