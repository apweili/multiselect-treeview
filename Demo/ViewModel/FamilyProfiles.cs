using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Enums;
using System.Windows.Interfaces;
using System.Windows.Models;

namespace Demo.ViewModel
{
    public class FamilyProfiles : IAutoBindExpandableModel,  IAutoBindImageSourceModel, INotifyPropertyChanged
    {
        private SelectionCheckState selectionCheckState;
        public string Name { get; set; }
        public IAutoBindExpandableModel Parent { get; set; } 
        public IEnumerable<IAutoBindExpandableModel> Children { get; set; }

        public FamilyProfiles(IAutoBindExpandableModel parent = null)
        {
            Parent = parent;
        }
        
        private bool _isExpanded { get; set; }

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                _isExpanded = value;
                OnPropertyChanged();
            }
        }

        public SelectionCheckState SelectionCheckState
        {
            get => selectionCheckState;
            set
            {
                if (value == selectionCheckState) return;
                selectionCheckState = value;
                OnPropertyChanged();
            }
        }

        public object ImageSource { get; set; } = "C:\\Users\\lw\\Desktop\\GroupController.png"; //20120628172839.jpg GroupController.png";
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}