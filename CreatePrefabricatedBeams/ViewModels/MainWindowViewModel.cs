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

        #region Элементы линии на поверхности 1
        private string _roadLineElemIds1;

        public string RoadLineElemIds1
        {
            get => _roadLineElemIds1;
            set => Set(ref _roadLineElemIds1, value);
        }
        #endregion

        #region Элементы линии на поверхности 2
        private string _roadLineElemIds2;

        public string RoadLineElemIds2
        {
            get => _roadLineElemIds2;
            set => Set(ref _roadLineElemIds2, value);
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

        #region Получение линии на поверхности дороги 1
        public ICommand GetRoadLines1 { get; }

        private void OnGetRoadLines1CommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetRoadLine1();
            RoadLineElemIds1 = RevitModel.RoadLineElemIds1;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadLines1CommandExecute(object parameter)
        {
            return true;
        }
        #endregion

        #region Получение линии на поверхности дороги 2
        public ICommand GetRoadLines2 { get; }

        private void OnGetRoadLines2CommandExecuted(object parameter)
        {
            RevitCommand.mainView.Hide();
            RevitModel.GetRoadLine2();
            RoadLineElemIds2 = RevitModel.RoadLineElemIds2;
            RevitCommand.mainView.ShowDialog();
        }

        private bool CanGetRoadLines2CommandExecute(object parameter)
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
            Properties.Settings.Default.RoadLineElemIds1 = RoadLineElemIds1;
            Properties.Settings.Default.RoadLineElemIds2 = RoadLineElemIds2;
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

            #region Инициализация значения элементам линии на поверхности 1
            if (!(Properties.Settings.Default.RoadLineElemIds1 is null))
            {
                string line1ElementIdInSettings = Properties.Settings.Default.RoadLineElemIds1.ToString();
                if (RevitModel.IsLinesExistInModel(line1ElementIdInSettings) && !string.IsNullOrEmpty(line1ElementIdInSettings))
                {
                    RoadLineElemIds1 = line1ElementIdInSettings;
                    RevitModel.GetRoadLines1BySettings(line1ElementIdInSettings);
                }
            }
            #endregion

            #region Инициализация значения элементам линии на поверхности 2
            if (!(Properties.Settings.Default.RoadLineElemIds2 is null))
            {
                string line2ElementIdInSettings = Properties.Settings.Default.RoadLineElemIds2.ToString();
                if (RevitModel.IsLinesExistInModel(line2ElementIdInSettings) && !string.IsNullOrEmpty(line2ElementIdInSettings))
                {
                    RoadLineElemIds2 = line2ElementIdInSettings;
                    RevitModel.GetRoadLines2BySettings(line2ElementIdInSettings);
                }
            }
            #endregion

            #region Команды
            GetBeamElementsCommand = new LambdaCommand(OnGetBeamElementsCommandExecuted, CanGetBeamElementsCommandExecute);

            GetRoadLines1 = new LambdaCommand(OnGetRoadLines1CommandExecuted, CanGetRoadLines1CommandExecute);

            GetRoadLines2 = new LambdaCommand(OnGetRoadLines2CommandExecuted, CanGetRoadLines2CommandExecute);

            CloseWindowCommand = new LambdaCommand(OnCloseWindowCommandExecuted, CanCloseWindowCommandExecute);
            #endregion
        }

        public MainWindowViewModel() { }
        #endregion
    }
}
