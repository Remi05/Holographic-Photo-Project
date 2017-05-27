// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Windows.Input;

namespace HolographicPhotoProject
{
    /// <summary>
    /// Command whose sole purpose is to relay its functionality to other objects by invoking delegates.
    /// </summary>
    public class RelayCommand<T> : ICommand
    {
        private Action<T> action = null;
        private Predicate<T> canExecute = null;

        public RelayCommand(Action<T> action, Predicate<T> canExecute)
        {
            this.action = action;
            this.canExecute = canExecute;
        }

        /// <summary>
        /// Determines whether or not the command is able to be executed based on a condition. The default return value is true.
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return canExecute == null ? true : canExecute((T)parameter);
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Executes the command.
        /// </summary>
        public void Execute(object parameter)
        {
            action((T)parameter);
        }

        /// <summary>
        /// Raise the CanExecuteChanged event to recheck whether or not the command should execute.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}