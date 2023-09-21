using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CreatePrefabricatedBeams.Infrastructure;

namespace CreatePrefabricatedBeams.ViewModels
{
    internal class MainWindowViewModel : Base.ViewModel
    {
        private RevitModelForfard _revitModel;

        internal RevitModelForfard RevitModel
        {
            get => _revitModel;
            set => _revitModel = value;
        }

        #region Заголовок
        private string _title = "Сборные балки";
        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        #endregion

        #region Блоки пролетного строения
        private string _beamElementIds;
        public string BeamElementIds
        {
            get => _beamElementIds;
            set => Set(ref _beamElementIds, value);
        }
        #endregion

        #region Команды

        #region Получение пользователем балок пролетного строения
        public ICommand GetBeamElementsCommand { get; }

        private void OnGetBeamElementsCommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetBeamElementsBySelection();
            BeamElementIds = RevitModel.BeamElementIds;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetBeamElementsCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion


        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Команды
            GetBeamElementsCommand = new LambdaCommand(OnGetBeamElementsCommandExecuted, CanGetBeamElementsCommandExecute);

            #endregion
        }

        public MainWindowViewModel() { }
        #endregion
    }
}
