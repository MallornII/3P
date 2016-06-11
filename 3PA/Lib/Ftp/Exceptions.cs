﻿#region header
// ========================================================================
// Copyright (c) 2016 - Julien Caillon (julien.caillon@gmail.com)
// This file (Exceptions.cs) is part of 3P.
// 
// 3P is a free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// 3P is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with 3P. If not, see <http://www.gnu.org/licenses/>.
// ========================================================================
#endregion
using System;

namespace _3PA.Lib.Ftp {
    /*
     *  Copyright 2008 Alessandro Pilotti
     *
     *  This program is free software; you can redistribute it and/or modify
     *  it under the terms of the GNU Lesser General Public License as published by
     *  the Free Software Foundation; either version 2.1 of the License, or
     *  (at your option) any later version.
     *
     *  This program is distributed in the hope that it will be useful,
     *  but WITHOUT ANY WARRANTY; without even the implied warranty of
     *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
     *  GNU Lesser General Public License for more details.
     *
     *  You should have received a copy of the GNU Lesser General Public License
     *  along with this program; if not, write to the Free Software
     *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA 
     */

    /// <summary>
    /// Base FTP exception class.
    /// </summary>
    public class FtpException : Exception {
        protected FtpException() {
        }

        public FtpException(string message)
            : base(message) {
        }

        public FtpException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }

    public class FtpReplyParseException : FtpException {
        private string _replyText;

        public string ReplyText {
            get { return _replyText; }
        }

        public FtpReplyParseException(string replyText)
            : base("Invalid server reply: " + replyText) {
            _replyText = replyText;
        }
    }

    public class FtpProtocolException : FtpException {
        FtpReply _reply;

        public FtpReply Reply {
            get { return _reply; }
        }

        public FtpProtocolException(FtpReply reply)
            : base("Invalid FTP protocol reply: " + reply) {
            _reply = reply;
        }
    }

    /// <summary>
    /// Exception indicating that a command or set of commands have been cancelled by the caller, via a callback method or event.
    /// </summary>
    public class FtpOperationCancelledException : FtpException {
        public FtpOperationCancelledException(string message)
            : base(message) {
        }
    }

    /// <summary>
    /// FTP exception generated by a command with a return code >= 400, as stated in RFC 959.
    /// </summary>
    public class FtpCommandException : FtpException {
        int _errorCode;

        public int ErrorCode {
            get { return _errorCode; }
        }

        public FtpCommandException(string message)
            : base(message) {
        }

        public FtpCommandException(string message, Exception innerException)
            : base(message, innerException) {
        }

        public FtpCommandException(FtpReply reply)
            : base(reply.Message) {
            _errorCode = reply.Code;
        }
    }

    /// <summary>
    /// FTP exception related to the SSL/TLS support
    /// </summary>
    public class FtpSslException : FtpException {
        public FtpSslException(string message)
            : base(message) {
        }

        public FtpSslException(string message, Exception innerException)
            : base(message, innerException) {
        }
    }
}
