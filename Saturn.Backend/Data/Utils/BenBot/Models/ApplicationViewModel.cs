using System;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using MudBlazor;
using Oodle.NET;
using Saturn.Backend.Data.Enums;

namespace Saturn.Backend.Data.Utils.BenBot.Models;

public class ApplicationViewModel : ViewModel
    {
        private bool _isReady;
        public bool IsReady
        {
            get => _isReady;
            private set => SetProperty(ref _isReady, value);
        }

        private EStatusKind _status;
        public EStatusKind Status
        {
            get => _status;
            set
            {
                SetProperty(ref _status, value);
                IsReady = Status != EStatusKind.Loading && Status != EStatusKind.Stopping;
            }
        }

        
        public ApplicationViewModel()
        {
            Status = EStatusKind.Ready;
        }
    }