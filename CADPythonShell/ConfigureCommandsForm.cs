﻿using System.IO;

namespace CADPythonShell
{
    public partial class ConfigureCommandsForm : Form
    {
        private List<Command> _commands;
        private List<string> _searchPaths;
        private List<KeyValuePair<string, string>> _variables;

        public ConfigureCommandsForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Close the Dialog without saving changes.
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Read in the commands from the XML file and display them in the
        /// list.
        /// </summary>
        private void ConfigureCommandsForm_Load(object sender, EventArgs e)
        {
            _commands = CADPythonShellApplication.GetCommands(
                CADPythonShellApplication.GetSettings()).ToList();
            lstCommands.DataSource = _commands;

            _searchPaths = CADPythonShellApplication.GetConfig().GetSearchPaths().ToList();
            lstSearchPaths.DataSource = _searchPaths;

            _variables = CADPythonShellApplication.GetConfig().GetVariables().AsEnumerable().ToList();
            lstVariables.DataSource = _variables;
            lstVariables.DisplayMember = "Key";

            string initScriptPath = CADPythonShellApplication.GetInitScriptPath();
            txtInitScript.Text = initScriptPath;

            string startupScriptPath = CADPythonShellApplication.GetStartupScriptPath();
            txtStartupScript.Text = startupScriptPath;
        }

        /// <summary>
        /// Display information about the selected command in the textboxes (Name, Path).
        /// </summary>
        private void lstCommands_SelectedIndexChanged(object sender, EventArgs e)
        {
            var command = (Command)lstCommands.SelectedItem;
            txtCommandName.Text = command.Name;
            txtCommandPath.Text = command.Source;
            txtCommandGroup.Text = command.Group;
        }

        /// <summary>
        /// Update changes in list.
        /// </summary>
        private void txtCommandName_TextChanged(object sender, EventArgs e)
        {
            var command = (Command)lstCommands.SelectedItem;
            command.Name = txtCommandName.Text;

            RefreshBindingContext(lstCommands, _commands);
        }

        private void txtCommandGroup_TextChanged(object sender, EventArgs e)
        {
            var command = (Command)lstCommands.SelectedItem;
            command.Group = txtCommandGroup.Text;

            RefreshBindingContext(lstCommands, _commands);
        }

        /// <summary>
        /// Update changes in list.
        /// </summary>
        private void txtCommandPath_TextChanged(object sender, EventArgs e)
        {
            var command = (Command)lstCommands.SelectedItem;
            command.Source = txtCommandPath.Text;

            RefreshBindingContext(lstCommands, _commands);
        }

        /// <summary>
        /// show a FileOpen Dialog.
        /// </summary>
        private void btnCommandBrowse_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;
            dialog.FileName = txtCommandPath.Text;

            dialog.ShowDialog(this);
            txtCommandPath.Text = dialog.FileName;
        }

        /// <summary>
        /// Add a new command.
        /// </summary>
        private void btnCommandAdd_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;

            // chances are, the user wants to add a script from the same folder
            // as the other scripts
            if (_commands.Count > 0)
            {
                dialog.InitialDirectory = Path.GetDirectoryName(_commands.First().Source);
            }

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                var command = new Command();
                command.Name = "";
                command.Group = "";
                command.Source = dialog.FileName;

                _commands.Add(command);

                RefreshBindingContext(lstCommands, _commands);

                lstCommands.SelectedIndex = _commands.Count - 1;

                txtCommandPath.Text = command.Source;
                txtCommandName.Text = command.Name;
                txtCommandName.Focus();
            }
        }

        /// <summary>
        /// Add a new search path.
        /// </summary>
        private void btnSearchPathAdd_Click(object sender, EventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _searchPaths.Add(dialog.SelectedPath);

                RefreshBindingContext(lstSearchPaths, _searchPaths);
                lstSearchPaths.SelectedIndex = _searchPaths.Count - 1;
                txtSearchPath.Text = dialog.SelectedPath;
            }
        }

        private void RefreshBindingContext(ListBox listBox, object dataSource)
        {
            ((CurrencyManager)listBox.BindingContext[dataSource]).Refresh();
        }

        /// <summary>
        /// Remove the selected item.
        /// </summary>
        private void btnCommandRemove_Click(object sender, EventArgs e)
        {
            if (lstCommands.SelectedIndex >= 0)
            {
                _commands.RemoveAt(lstCommands.SelectedIndex);
                RefreshBindingContext(lstCommands, _commands);
            }
        }

        /// <summary>
        /// Move the selected item up one.
        /// </summary>
        private void btnCommandMoveUp_Click(object sender, EventArgs e)
        {
            int oldPosition = lstCommands.SelectedIndex;
            int newPosition = Math.Max(0, oldPosition - 1);

            SwapPositions(_commands, oldPosition, newPosition);

            RefreshBindingContext(lstCommands, _commands);

            lstCommands.SelectedIndex = newPosition;
        }

        private void SwapPositions<T>(List<T> container, int oldPosition, int newPosition)
        {
            var temp = container[newPosition];
            container[newPosition] = container[oldPosition];
            container[oldPosition] = temp;
        }

        private void btnCommandMoveDown_Click(object sender, EventArgs e)
        {
            int oldPosition = lstCommands.SelectedIndex;
            int newPosition = Math.Min(_commands.Count - 1, oldPosition + 1);

            SwapPositions(_commands, oldPosition, newPosition);

            RefreshBindingContext(lstCommands, _commands);

            lstCommands.SelectedIndex = newPosition;
        }

        /// <summary>
        /// Save changes to CADPythonShell.xml and close Dialog.
        /// </summary>
        private void btnCommandSave_Click(object sender, EventArgs e)
        {
            CADPythonShellApplication.WriteSettings(_commands, _searchPaths, _variables, txtInitScript.Text, txtStartupScript.Text);
            Close();
        }

        private void btnSearchPathRemove_Click(object sender, EventArgs e)
        {
            if (lstSearchPaths.SelectedIndex >= 0)
            {
                _searchPaths.RemoveAt(lstSearchPaths.SelectedIndex);
                RefreshBindingContext(lstSearchPaths, _searchPaths);
            }
        }

        private void btnSearchPathBrowse_Click(object sender, EventArgs e)
        {
            if (lstSearchPaths.SelectedIndex < 0)
            {
                return;
            }

            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                _searchPaths[lstSearchPaths.SelectedIndex] = dialog.SelectedPath;

                RefreshBindingContext(lstSearchPaths, _searchPaths);
                txtSearchPath.Text = dialog.SelectedPath;
            }
        }

        /// <summary>
        /// Show the currently selected search path in the textbox for editing.
        /// </summary>
        private void lstSearchPaths_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstSearchPaths.SelectedIndex < 0)
            {
                return;
            }

            txtSearchPath.Text = _searchPaths[lstSearchPaths.SelectedIndex];
        }

        /// <summary>
        /// Update the search path to reflect changes.
        /// </summary>
        private void txtSearchPath_TextChanged(object sender, EventArgs e)
        {
            if (lstSearchPaths.SelectedIndex < 0)
            {
                return;
            }

            _searchPaths[lstSearchPaths.SelectedIndex] = txtSearchPath.Text;
            RefreshBindingContext(lstSearchPaths, _searchPaths);
        }

        /// <summary>
        /// Display selected variable in the textboxes for editing.
        /// </summary>
        private void lstVariables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstVariables.SelectedIndex < 0)
            {
                return;
            }

            var variable = _variables[lstVariables.SelectedIndex];
            txtVariableName.Text = variable.Key;
            txtVariableValue.Text = variable.Value;
        }

        /// <summary>
        /// Update variable name to reflect changes in textbox.
        /// </summary>
        private void txtVariableName_TextChanged(object sender, EventArgs e)
        {
            if (lstVariables.SelectedIndex < 0)
            {
                return;
            }

            var variable = _variables[lstVariables.SelectedIndex];
            _variables[lstVariables.SelectedIndex] = new KeyValuePair<string, string>(txtVariableName.Text, variable.Value);
            RefreshBindingContext(lstVariables, _variables);
        }

        /// <summary>
        /// Update variable value to reflect changes in textbox.
        /// </summary>
        private void txtVariableValue_TextChanged(object sender, EventArgs e)
        {
            if (lstVariables.SelectedIndex < 0)
            {
                return;
            }

            var variable = _variables[lstVariables.SelectedIndex];
            _variables[lstVariables.SelectedIndex] = new KeyValuePair<string, string>(variable.Key, txtVariableValue.Text);
            RefreshBindingContext(lstVariables, _variables);
        }

        /// <summary>
        /// Add a new variable to the list and select it for editing.
        /// </summary>
        private void btnVariableAdd_Click(object sender, EventArgs e)
        {
            _variables.Add(new KeyValuePair<string, string>("<name>", "<value>"));
            RefreshBindingContext(lstVariables, _variables);

            lstVariables.SelectedIndex = _variables.Count - 1;
        }

        /// <summary>
        /// Remove variable from the list.
        /// </summary>
        private void btnVariableRemove_Click(object sender, EventArgs e)
        {
            if (lstVariables.SelectedIndex < 0)
            {
                return;
            }

            _variables.RemoveAt(lstVariables.SelectedIndex);
            RefreshBindingContext(lstVariables, _variables);
        }

        private void btnInitScriptBrowse_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;
            dialog.FileName = txtCommandPath.Text;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtInitScript.Text = dialog.FileName;
            }
        }

        private void btnStartupScriptBrowse_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.CheckFileExists = true;
            dialog.CheckPathExists = true;
            dialog.Multiselect = false;
            dialog.FileName = txtCommandPath.Text;

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                txtStartupScript.Text = dialog.FileName;
            }
        }
    }
}