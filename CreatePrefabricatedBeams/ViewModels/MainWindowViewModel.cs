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

        #region Высота балки
        private double _beamHeight = Properties.Settings.Default.BeamHeight;
        public double BeamHeight
        {
            get => _beamHeight;
            set => Set(ref _beamHeight, value);
        }
        #endregion

        #region Ширина балки
        private double _beamWidth = Properties.Settings.Default.BeamWidth;
        public double BeamWidth
        {
            get => _beamWidth;
            set => Set(ref _beamWidth, value);
        }
        #endregion

        #region Толщина покрытия
        private double _roadSurfaceThikness = Properties.Settings.Default.RoadSurfaceThikness;
        public double RoadSurfaceThikness
        {
            get => _roadSurfaceThikness;
            set => Set(ref _roadSurfaceThikness, value);
        }
        #endregion

        #region Толщина плиты
        private double _slabThikness = Properties.Settings.Default.SlabThikness;
        public double SlabThikness
        {
            get => _slabThikness;
            set => Set(ref _slabThikness, value);
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

        #region Закрыть окно
        public ICommand CloseWindowCommand { get; }

        private void OnCloseWindowCommandExecuted(object parameter)
        {
            SaveSettings();
            RevitCommand.mainView.Close();
        }

        private bool CanCloseWindowCommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #endregion

        private void SaveSettings()
        {
            Properties.Settings.Default.BeamElementIds = BeamElementIds;
            Properties.Settings.Default.BeamHeight = BeamHeight;
            Properties.Settings.Default.BeamWidth = BeamWidth;
            Properties.Settings.Default.RoadSurfaceThikness = RoadSurfaceThikness;
            Properties.Settings.Default.SlabThikness = SlabThikness;
            Properties.Settings.Default.Save();
        }

        #region Конструктор класса MainWindowViewModel
        public MainWindowViewModel(RevitModelForfard revitModel)
        {
            RevitModel = revitModel;

            #region Инициализация свойств из Settings

            #region Инициализация балок
            if (!(Properties.Settings.Default.BeamElementIds is null))
            {
                string beamElementIdsInSettings = Properties.Settings.Default.BeamElementIds;
                if (RevitModel.IsElementsExistInModel(beamElementIdsInSettings) && !string.IsNullOrEmpty(beamElementIdsInSettings))
                {
                    BeamElementIds = beamElementIdsInSettings;
                    RevitModel.GetBeamsBySettings(beamElementIdsInSettings);
                }
            }
            #endregion

            #endregion


            #region Команды
            GetBeamElementsCommand = new LambdaCommand(OnGetBeamElementsCommandExecuted, CanGetBeamElementsCommandExecute);

            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel() { }
        #endregion
    }
}
