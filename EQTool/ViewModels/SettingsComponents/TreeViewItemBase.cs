﻿using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace EQTool.ViewModels.SettingsComponents
{
    public enum TreeViewItemType
    {
        General,
        Player,
        Server,
        Trigger,
        Global,
        Zone
    }
    public enum MobInfoItemType
    {
        Mob,
        Pet
    }

    public abstract class TreeViewItemBase : INotifyPropertyChanged
    {
        protected TreeViewItemBase(TreeViewItemBase parent)
        {
            this.Parent = parent;
        }
        public abstract TreeViewItemType Type { get; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (value != isSelected)
                {
                    isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool isExpanded;
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (value != isExpanded)
                {
                    isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public TreeViewItemBase Parent { get; set; }
        public abstract string Name { get; }
        public ObservableCollection<TreeViewItemBase> Children { get; set; } = new ObservableCollection<TreeViewItemBase>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
