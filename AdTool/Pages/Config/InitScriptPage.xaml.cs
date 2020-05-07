using CsLib;
using LogClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AdTool.Core;


namespace AdTool
{
    /// <summary>
    /// InitScriptPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InitScriptPage : BasePage<InitScriptViewModel>
    {
        public InitScriptPage()
        {
            InitializeComponent();
        }
    }
}
