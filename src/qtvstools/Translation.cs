/****************************************************************************
**
** Copyright (C) 2016 The Qt Company Ltd.
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

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.VCProjectEngine;
using QtVsTools.Core;
using QtVsTools.VisualStudio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QtVsTools
{
    using static QtMsBuild.QtProjectTracker;
    using Property = KeyValuePair<string, string>;

    /// <summary>
    /// Run Qt translation tools by invoking the corresponding Qt/MSBuild targets
    /// </summary>
    public static class Translation
    {
        public static void RunlRelease(VCFile vcFile)
        {
            var vcProj = vcFile.project as VCProject;
            var project = vcProj?.Object as EnvDTE.Project;
            RunTranslationTarget(BuildAction.Release,
                project, new[] { vcFile.Name });
        }

        public static void RunlRelease(VCFile[] vcFiles)
        {
            var vcProj = vcFiles.FirstOrDefault()?.project as VCProject;
            var project = vcProj?.Object as EnvDTE.Project;
            RunTranslationTarget(BuildAction.Release,
                project, vcFiles.Select(vcFile => vcFile?.Name));
        }

        public static void RunlRelease(EnvDTE.Project project)
        {
            RunTranslationTarget(BuildAction.Release, project);
        }

        public static void RunlRelease(EnvDTE.Solution solution)
        {
            if (solution == null)
                return;

            foreach (var project in HelperFunctions.ProjectsInSolution(solution.DTE))
                RunlRelease(project);
        }

        public static void RunlUpdate(VCFile vcFile)
        {
            var vcProj = vcFile.project as VCProject;
            var project = vcProj?.Object as EnvDTE.Project;
            RunTranslationTarget(BuildAction.Update,
                project, new[] { vcFile.Name });
        }

        public static void RunlUpdate(VCFile[] vcFiles)
        {
            var vcProj = vcFiles.FirstOrDefault()?.project as VCProject;
            var project = vcProj?.Object as EnvDTE.Project;
            RunTranslationTarget(BuildAction.Update,
                project, vcFiles.Select(vcFile => vcFile?.Name));
        }

        public static void RunlUpdate(EnvDTE.Project project)
        {
            RunTranslationTarget(BuildAction.Update, project);
        }

        enum BuildAction { Update, Release }

        static void RunTranslationTarget(
            BuildAction buildAction,
            EnvDTE.Project project,
            IEnumerable<string> selectedFiles = null)
        {
            var qtPro = QtProject.Create(project);
            if (project == null || qtPro == null) {
                Messages.Print(
                    "translation: Error accessing project interface");
                return;
            }

            var activeConfig = project.ConfigurationManager?.ActiveConfiguration;
            if (activeConfig == null) {
                Messages.Print(
                    "translation: Error accessing build interface");
                return;
            }
            var activeConfigId = string.Format("{0}|{1}",
                activeConfig.ConfigurationName, activeConfig.PlatformName);

            var target = "QtTranslation";
            var properties = new List<Property>();
            switch (buildAction) {
                case BuildAction.Update:
                    properties.Add(PROPERTY("QtTranslationForceUpdate", "true"));
                    break;
                case BuildAction.Release:
                    properties.Add(PROPERTY("QtTranslationForceRelease", "true"));
                    break;
            }
            if (selectedFiles != null)
                properties.Add(PROPERTY("SelectedFiles", string.Join(";", selectedFiles)));

            Build(project, activeConfigId, properties.ToArray(), target);
        }

        public static void RunlUpdate(EnvDTE.Solution solution)
        {
            if (solution == null)
                return;

            foreach (var project in HelperFunctions.ProjectsInSolution(solution.DTE))
                RunlUpdate(project);
        }

        public static void CreateNewTranslationFile(EnvDTE.Project project)
        {
            if (project == null)
                return;

            using (var transDlg = new AddTranslationDialog(project)) {
                if (transDlg.ShowDialog() == DialogResult.OK) {
                    try {
                        var qtPro = QtProject.Create(project);
                        var file = qtPro.AddFileInFilter(Filters.TranslationFiles(),
                            transDlg.TranslationFile, true);
                        RunlUpdate(file);
                    } catch (QtVSException e) {
                        Messages.DisplayErrorMessage(e.Message);
                    } catch (System.Exception ex) {
                        Messages.DisplayErrorMessage(ex.Message);
                    }
                }
            }
        }
    }
}
