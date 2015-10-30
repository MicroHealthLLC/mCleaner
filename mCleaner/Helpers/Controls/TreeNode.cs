// http://www.codeproject.com/Articles/28306/Working-with-Checkboxes-in-the-WPF-TreeView

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace mCleaner.Helpers.Controls
{
    public delegate void OnTreeNodeChecked(object sender, EventArgs e);
    public delegate void OnTreeNodeSelected(object sender, EventArgs e);
    public class TreeNode : INotifyPropertyChanged
    {
        public event OnTreeNodeChecked TreeNodeChecked;
        public event OnTreeNodeSelected TreeNodeSelected;

        #region properties

        public List<TreeNode> Children { get; set; }

        private bool _IsInitiallySelected = false;
        public bool IsInitiallySelected
        {
            get { return _IsInitiallySelected; }
            set
            {
                if (_IsInitiallySelected != value)
                {
                    _IsInitiallySelected = value;

                    if (value == true)
                    {
                        if (TreeNodeSelected != null)
                        {
                            TreeNodeSelected(this, new EventArgs());
                        }
                    }
                }
            }
        }

        public string Key { get; set; }

        public string Name { get; set; }

        public object Tag { get; set; }

        public object IsAccordionHeader { get; set; }

        private bool _SupressWarningMessage = false;
        public bool SupressWarningMessage
        {
            get { return _SupressWarningMessage; }
            set
            {
                if (_SupressWarningMessage != value)
                {
                    _SupressWarningMessage = value;
                }
            }
        }

        private bool _IsExpanded = false;
        public bool IsExpanded
        {
            get { return _IsExpanded; }
            set
            {
                if (_IsExpanded != value)
                {
                    _IsExpanded = value;
                    this.OnPropertyChanged("IsExpanded");
                }
            }
        }

        /// <summary>
        /// Gets/sets the state of the associated UI toggle (ex. CheckBox).
        /// The return value is calculated based on the check state of all
        /// child FoeViewModels.  Setting this property to true or false
        /// will set all children to the same check state, and setting it 
        /// to any value will cause the parent to verify its check state.
        /// </summary>
        bool? _isChecked = false;
        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                this.SetIsChecked(value, true, true);

                if (TreeNodeChecked != null)
                {
                    TreeNodeChecked(this, new EventArgs());
                }
            }
        }

        private TreeNode _Parent;
        public TreeNode Parent
        {
            get { return _Parent; }
            set
            {
                if (_Parent != value)
                {
                    _Parent = value;
                }
            }
        }

        #endregion // Properties

        #region ctr
        public TreeNode(string name, string key)
        {
            this.Name = name;
            this.Key = key;
            this.Children = new List<TreeNode>();
        }

        public TreeNode(string name)
        {
            this.Name = name;
            this.Key = name;
            this.Children = new List<TreeNode>();
        }

        public TreeNode()
        {
        }
        #endregion

        #region methods
        public void Initialize()
        {
            foreach (TreeNode child in this.Children)
            {
                child.Parent = this;
                child.Initialize();
            }
        }

        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
                this.Children.ForEach(c => c.SetIsChecked(_isChecked, true, false));

            if (updateParent && Parent != null)
                Parent.VerifyCheckState();

            this.OnPropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }
        #endregion

        #region INotifyPropertyChanged Members

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
