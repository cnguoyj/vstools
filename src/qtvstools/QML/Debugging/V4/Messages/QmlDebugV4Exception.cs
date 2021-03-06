﻿/****************************************************************************
**
** Copyright (C) 2018 The Qt Company Ltd.
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

using System.Runtime.Serialization;

namespace QtVsTools.Qml.Debug.V4
{
    using Json;

    [DataContract]
    sealed class ExceptionEvent : Event<ExceptionEvent.BodyStruct>
    {
        //  "v8message"
        //  { "seq"   : <number>,
        //    "type"  : "event",
        //    "event" : "break",
        //    "body"  : { "sourceLine" : <int>,
        //                "script"     : { "name" : <string>
        //                },
        //                "text"       : <string>,
        //                "exception"  : <object>
        //              }
        //  }
        public const string EV_TYPE = "exception";
        public ExceptionEvent() : base()
        {
            EventType = EV_TYPE;
        }

        [DataContract]
        public class BodyStruct
        {
            [DataMember(Name = "sourceLine")]
            public int SourceLine { get; set; }

            [DataMember(Name = "script")]
            public ScriptStruct Script { get; set; }

            [DataMember(Name = "text")]
            public string Text { get; set; }

            [DataMember(Name = "exception")]
            public DeferredObject<JsValue> Exception { get; set; }

            [DataContract]
            public class ScriptStruct
            {
                [DataMember(Name = "name")]
                public string Name { get; set; }
            }
        }
    }
}
