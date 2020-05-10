/*
    Copyright (C) 2014-2019 de4dot@gmail.com

    This file is part of dnSpy

    dnSpy is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    dnSpy is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with dnSpy.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Windows.Input;

namespace dnSpy.Contracts.MVVM {
	/// <summary>
	/// Implements the <see cref="ICommand"/> interface
	/// </summary>
	public class RelayCommand : ICommand {
		readonly Action<object?> exec;
		readonly Predicate<object?>? canExec;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="exec">Called when the command gets executed</param>
		/// <param name="canExec">Gets called to check whether <paramref name="exec"/> can execute,
		/// may be null</param>
		public RelayCommand(Action<object?> exec, Predicate<object?>? canExec = null) {
			this.exec = exec ?? throw new ArgumentNullException(nameof(exec));
			this.canExec = canExec;
		}

		bool ICommand.CanExecute(object? parameter) => canExec is null || canExec(parameter);

		event EventHandler? ICommand.CanExecuteChanged {
			add => CommandManager.RequerySuggested += value;
			remove => CommandManager.RequerySuggested -= value;
		}

		void ICommand.Execute(object? parameter) => exec(parameter);
	}

	/// <summary>
	/// Implements the <see cref="ICommand"/> interface with a strongly-typed parameter.
	/// </summary>
	/// <typeparam name="TParameter">The type of command parameter.</typeparam>
	public class RelayCommand<TParameter> : RelayCommand {
		/// <inheritdoc />
		/// <remarks>When passed parameter is not a <typeparamref name="TParameter"/>,
		/// <paramref name="exec"/> and <paramref name="canExec"/> are not called,
		/// and <see cref="ICommand.CanExecute"/> returns <c>false</c>.</remarks>
		public RelayCommand(Action<TParameter> exec, Predicate<TParameter>? canExec = null)
			: base(MakeExecute(exec), MakeCanExecute(canExec)) {
		}

		static Action<object?> MakeExecute(Action<TParameter> exec) {
			if(exec is null) throw new ArgumentNullException(nameof(exec));
			return p => {
				if (p is TParameter parameter) exec(parameter);
			};
		}

		static Predicate<object?>? MakeCanExecute(Predicate<TParameter>? canExec) {
			if (canExec is null) return p => p is TParameter;
			return p => p is TParameter parameter && canExec(parameter);
		}
	}
}
