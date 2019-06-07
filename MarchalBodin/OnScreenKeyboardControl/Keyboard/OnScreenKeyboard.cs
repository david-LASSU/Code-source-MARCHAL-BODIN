using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OnScreenKeyboardControl.Keyboard.Keys;

namespace OnScreenKeyboardControl.Keyboard
{
    public class OnScreenKeyboard : Grid
    {
        #region dependency properties

        public static readonly DependencyProperty ToggleButtonStyleProperty = DependencyProperty.Register("ToggleButtonStyle",
            typeof(Style), typeof(OnScreenKeyboard), new PropertyMetadata(null));

        public Style ToggleButtonStyle
        {
            get { return (Style)GetValue(ToggleButtonStyleProperty); }
            set { SetValue(ToggleButtonStyleProperty, value); }
        }

        public static readonly DependencyProperty SaveCommandProperty = DependencyProperty.Register("SaveCommand",
            typeof(ICommand), typeof(OnScreenKeyboard), new PropertyMetadata(null));

        public ICommand SaveCommand
        {
            get { return (ICommand)GetValue(SaveCommandProperty); }
            set { SetValue(SaveCommandProperty, value); }
        }

        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand",
            typeof(ICommand), typeof(OnScreenKeyboard), new PropertyMetadata(null));

        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelCommandProperty); }
            set { SetValue(CancelCommandProperty, value); }
        }

        #endregion

        #region ActiveControl region

        public FrameworkElement ActiveContainer
        {
            get { return (FrameworkElement)GetValue(ActiveContainerProperty); }
            set { SetValue(ActiveContainerProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveContainer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveContainerProperty =
            DependencyProperty.Register("ActiveContainer", typeof(FrameworkElement), typeof(OnScreenKeyboard),
                new PropertyMetadata(null, OnActiveContainerChanged));

        private object ActiveControl
        {
            get { return GetValue(ActiveControlProperty); }
            set { SetValue(ActiveControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveControl.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty ActiveControlProperty =
            DependencyProperty.Register("ActiveControl", typeof(object), typeof(OnScreenKeyboard), new PropertyMetadata(null));


        private object KeyboardControl
        {
            get { return GetValue(KeyboardControlProperty); }
            set { SetValue(KeyboardControlProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyboardControl.  This enables animation, styling, binding, etc...
        private static readonly DependencyProperty KeyboardControlProperty =
            DependencyProperty.Register("KeyboardControl", typeof(OnScreenKeyboard), typeof(OnScreenKeyboard),
                new PropertyMetadata(null));

        private static void OnActiveContainerChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is FrameworkElement)
            {
                var frameworkElement = (FrameworkElement)e.NewValue;
                var keyboardControl = (OnScreenKeyboard)dependencyObject;
                //keyboardControl.CanType = true;
                frameworkElement.GotKeyboardFocus += FrameworkElement_GotKeyboardFocus;
                frameworkElement.SetValue(KeyboardControlProperty, dependencyObject);
            }
            else if (e.OldValue is Control)
            {
                var frameworkElement = (FrameworkElement)e.OldValue;
                frameworkElement.GotKeyboardFocus -= FrameworkElement_GotKeyboardFocus;
            }
        }

        private static void FrameworkElement_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var dependencyObject = (DependencyObject)sender;
            var keyboardControl = (OnScreenKeyboard)dependencyObject.GetValue(KeyboardControlProperty);
            keyboardControl._activeControl = (FrameworkElement)e.NewFocus;
            //keyboardControl.CanType = e.NewFocus is TextBoxBase || e.NewFocus is PasswordBox;
        }

        private FrameworkElement _activeControl;

        #endregion

        private bool? CanType
        {
            get { return _canType; }
            set
            {
                if (_canType != value)
                {
                    _canType = value;
                    _allOnScreenKeys.ForEach(i => i.CanType(value == true));
                }
            }
        }

        private bool? _canType;

        protected readonly List<OnScreenKeyboardSection> _sections = new List<OnScreenKeyboardSection>();
        protected readonly List<OnScreenKey> _allOnScreenKeys = new List<OnScreenKey>();
        private readonly List<OnScreenKeyStateModifier> _activeKeyModifiers = new List<OnScreenKeyStateModifier>();

        public override void BeginInit()
        {
            SetValue(FocusManager.IsFocusScopeProperty, true);

            var mainSection = new OnScreenKeyboardSection();
            var mainKeys = new ObservableCollection<OnScreenKey>
            {
                new OnScreenKeyNormal(0, 00, new[] {"`", "~", "∞", "≈"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 01, new[] {"1", "!", "§", "¡"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 02, new[] {"2", "@", "☼"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 03, new[] {"3", "#", "†", "‡"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 04, new[] {"4", "$", "£", "¢" }, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 05, new[] {"5", "%", "§", "¶"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 06, new[] {"6", "^", "∈", "°" }, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 07, new[] {"7", "&", "∩", "⊄"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 08, new[] {"8", "*", "∪"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 09, new[] {"9", "(", "⊂", "⊆" }, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 10, new[] {"0", ")", "⊃", "⊇" }, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 11, new[] {"-", "_", "×", "Ø"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(0, 12, new[] {"=", "+", "≠", "±" }, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeySpecial(0, 13, "Bksp", ExecuteDelegateFunctions.BackspaceExecuteDelegate) {GridWidth = new GridLength(2, GridUnitType.Star)},

                new OnScreenKeySpecial(1, 00, "Tab", ExecuteDelegateFunctions.MoveNextExecuteDelegate) {GridWidth = new GridLength(1.5, GridUnitType.Star)},
                new OnScreenKeyNormal(1, 01, new[] {"q", "Q", "θ", "Θ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 02, new[] {"w", "W","ω","Ω"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 03, new[] {"e", "E", "ε"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 04, new[] {"r", "R","ρ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 05, new[] {"t", "T"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 06, new[] {"y", "Y", "ψ", "Ψ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 07, new[] {"u", "U"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 08, new[] {"i", "I"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 09, new[] {"o", "O"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 10, new[] {"p", "P", "π", "Π"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 11, new[] {"[", "{", "‹", "«"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 12, new[] {"]", "}", "›", "»"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(1, 13, new[] {"\\", "|"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial)
                {
                    GridWidth = new GridLength(1.3, GridUnitType.Star)
                },

                new OnScreenKeyToggle(2, 00, new[] {"Caps"}, OnScreenKeyModifierType.Shift)
                {
                    GridWidth = new GridLength(1.7, GridUnitType.Star)
                },
                new OnScreenKeyNormal(2, 01, new[] {"a", "A", "α"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 02, new[] {"s", "S", "σ", "Σ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 03, new[] {"d", "D", "δ", "Δ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 04, new[] {"f", "F"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 05, new[] {"g", "G", "γ", "Γ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 06, new[] {"h", "H", "φ", "Φ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 07, new[] {"j", "J"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 08, new[] {"k", "K"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 09, new[] {"l", "L", "λ", "Λ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 10, new[] {";", ":", "♠", "♣"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(2, 11, new[] {"\"", "'", "♥", "♦"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeySpecial(2, 12, "Enter", ExecuteDelegateFunctions.MoveNextExecuteDelegate) {GridWidth = new GridLength(1.8, GridUnitType.Star)},

                new OnScreenKeyModifier(3, 00, new[] {"Shift"}, OnScreenKeyModifierType.Shift){GridWidth = new GridLength(2.4, GridUnitType.Star)},
                new OnScreenKeyNormal(3, 01, new[] {"z", "Z", "ζ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(3, 02, new[] {"x", "X", "ξ", "Ξ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(3, 03, new[] {"c", "C", "χ", "Χ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(3, 04, new[] {"v", "V"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(3, 05, new[] {"b", "B", "β"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(3, 06, new[] {"n", "N"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(3, 07, new[] {"m", "M", "μ"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(3, 08, new[] {",", "<", "≤"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(3, 09, new[] {".", ">", "≥"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyNormal(3, 10, new[] {"/", "?"}, CaptionUpdateDelegateDelegateFunction.ShiftAndSpecial),
                new OnScreenKeyModifier(3, 11, new[] {"Shift"}, OnScreenKeyModifierType.Shift){GridWidth = new GridLength(2.4, GridUnitType.Star)       },

                new OnScreenKeySpecial(4, 00,  "Clear", ExecuteDelegateFunctions.ClearExecuteDelegate),
                new OnScreenKeyModifier(4, 01, new [] { "Special"}, OnScreenKeyModifierType.Special),
                new OnScreenKeySpecial(4, 02,  string.Empty, " "){GridWidth = new GridLength(5, GridUnitType.Star)},
                new OnScreenKeySpecial(4, 03,  "Save",  nameof(SaveCommand)) {ClickCommand = nameof(SaveCommand)},
                new OnScreenKeySpecial(4, 04, "Cancel",  nameof(CancelCommand)) {ClickCommand = nameof(CancelCommand)},
            };

            mainSection.Keys = mainKeys;
            mainSection.SetValue(ColumnProperty, 0);
            _sections.Add(mainSection);
            ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            Children.Add(mainSection);
            _allOnScreenKeys.AddRange(mainSection.Keys);

            //var specialSection = new OnScreenKeyboardSection();
            //var specialKeys = new ObservableCollection<OnScreenKey>
            //                      {
            //                          new OnScreenKeyNormal ( 0,  0, Key = new ChordKey("Select All", VirtualKeyCode.CONTROL, VirtualKeyCode.VK__A), GridWidth = new GridLength(2, GridUnitType.Star)},
            //                          new OnScreenKeyNormal ( 0,  1, Key = new ChordKey("Undo", VirtualKeyCode.CONTROL, VirtualKeyCode.VK__Z) },
            //                          new OnScreenKeyNormal ( 1,  0, Key = new ChordKey("Copy", VirtualKeyCode.CONTROL, VirtualKeyCode.VK__C) },
            //                          new OnScreenKeyNormal ( 1,  1, Key = new ChordKey("Cut", VirtualKeyCode.CONTROL, VirtualKeyCode.VK__X) },
            //                          new OnScreenKeyNormal ( 1,  2, Key = new ChordKey("Paste", VirtualKeyCode.CONTROL, VirtualKeyCode.VK__V) },
            //                          new OnScreenKeyNormal ( 2,  0, Key = new VirtualKey(VirtualKeyCode.DELETE, "Del") },
            //                          new OnScreenKeyNormal ( 2,  1, Key = new VirtualKey(VirtualKeyCode.HOME, "Home") },
            //                          new OnScreenKeyNormal ( 2,  2, Key = new VirtualKey(VirtualKeyCode.END, "End") },
            //                          new OnScreenKeyNormal ( 3,  0, Key = new VirtualKey(VirtualKeyCode.PRIOR, "PgUp") },
            //                          new OnScreenKeyNormal ( 3,  1, Key = new VirtualKey(VirtualKeyCode.UP, "Up") },
            //                          new OnScreenKeyNormal ( 3,  2, Key = new VirtualKey(VirtualKeyCode.NEXT, "PgDn") },
            //                          new OnScreenKeyNormal ( 4,  0, Key = new VirtualKey(VirtualKeyCode.LEFT, "Left") },
            //                          new OnScreenKeyNormal ( 4,  1, Key = new VirtualKey(VirtualKeyCode.DOWN, "Down") },
            //                          new OnScreenKeyNormal ( 4,  2, Key = new VirtualKey(VirtualKeyCode.RIGHT, "Right") },
            //                      };

            //specialSection.Keys = specialKeys;
            //specialSection.SetValue(ColumnProperty, 1);
            //_sections.Add(specialSection);
            //ColumnDefinitions.Add(new ColumnDefinition());
            //Children.Add(specialSection);
            //_allOnScreenKeys.AddRange(specialSection.Keys);

            Loaded += OnScreenKeyboard_Loaded;

            base.BeginInit();
        }

        protected void OnScreenKeyboard_Loaded(object sender, RoutedEventArgs e)
        {
            _allOnScreenKeys.ForEach(x =>
            {
                x.Style = ToggleButtonStyle;
                x.OnScreenKeyPressEvent += OnScreenKeyPressEventHandler;
            });
        }

        void OnScreenKeyPressEventHandler(object sender, OnScreenKeyPressEventArgs e)
        {
            if (e.StateModifier == null)
            {
                e.Execute(_activeControl);
                var singleInstance = _activeKeyModifiers.Where(i => i.SingleInstance).Select(k => k).ToList();
                singleInstance.ForEach(j => _activeKeyModifiers.Remove(j));
            }
            else
            {
                var dups = _activeKeyModifiers.Where(i => i.ModifierType == e.StateModifier.ModifierType).Select(k => k).ToList();
                dups.ForEach(j => _activeKeyModifiers.Remove(j));
                if (e.StateModifier.Clear == false)
                    _activeKeyModifiers.Add(e.StateModifier);
            }
            _allOnScreenKeys.ForEach(i => i.Update(_activeKeyModifiers));
        }
    }
}