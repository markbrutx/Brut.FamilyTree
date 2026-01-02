using Brut.FamilyTree.Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.ScreenSystem;

namespace Brut.FamilyTree.ViewModels
{
    public class FamilyTreeVM : ViewModel
    {
        private readonly ScreenBase _parentScreen;
        private GauntletLayer? _popupLayer;
        private GauntletMovieIdentifier? _popupMovie;

        private bool _isPopupVisible;
        private string _familyTreeText = null!;
        private string _popupTitleText = null!;
        private string _closeButtonText = null!;
        private FamilyTreeNodeVM? _familyTree;

        public FamilyTreeVM(ScreenBase parentScreen)
        {
            _parentScreen = parentScreen;
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            FamilyTreeText = LocalizationHelper.GetString("str_family_tree");
            PopupTitleText = LocalizationHelper.GetString("str_family_tree");
            CloseButtonText = LocalizationHelper.GetString("str_close");
        }

        [DataSourceProperty]
        public bool IsPopupVisible
        {
            get => _isPopupVisible;
            set
            {
                if (value != _isPopupVisible)
                {
                    _isPopupVisible = value;
                    OnPropertyChangedWithValue(value, nameof(IsPopupVisible));
                }
            }
        }

        [DataSourceProperty]
        public string FamilyTreeText
        {
            get => _familyTreeText;
            set
            {
                if (value != _familyTreeText)
                {
                    _familyTreeText = value;
                    OnPropertyChangedWithValue(value, nameof(FamilyTreeText));
                }
            }
        }

        [DataSourceProperty]
        public string PopupTitleText
        {
            get => _popupTitleText;
            set
            {
                if (value != _popupTitleText)
                {
                    _popupTitleText = value;
                    OnPropertyChangedWithValue(value, nameof(PopupTitleText));
                }
            }
        }

        [DataSourceProperty]
        public string CloseButtonText
        {
            get => _closeButtonText;
            set
            {
                if (value != _closeButtonText)
                {
                    _closeButtonText = value;
                    OnPropertyChangedWithValue(value, nameof(CloseButtonText));
                }
            }
        }

        [DataSourceProperty]
        public FamilyTreeNodeVM? FamilyTree
        {
            get => _familyTree;
            set
            {
                if (value != _familyTree)
                {
                    _familyTree = value;
                    OnPropertyChangedWithValue(value, nameof(FamilyTree));
                }
            }
        }

        public void ExecuteOpenFamilyTree()
        {
            if (_popupLayer == null)
            {
                // Build family tree from player's hero
                var hero = Hero.MainHero;
                var rootHero = HeroHelper.FindAncestorOf(hero);
                FamilyTree = new FamilyTreeNodeVM(rootHero, hero);

                _popupLayer = new GauntletLayer("FamilyTreePopupLayer", 200);
                _popupMovie = _popupLayer.LoadMovie("FamilyTreePopup", this);
                _popupLayer.InputRestrictions.SetInputRestrictions();
                _parentScreen.AddLayer(_popupLayer);
                IsPopupVisible = true;
            }
        }

        public void ExecuteCloseFamilyTree()
        {
            if (_popupLayer != null)
            {
                _popupLayer.ReleaseMovie(_popupMovie);
                _parentScreen.RemoveLayer(_popupLayer);
                _popupLayer.InputRestrictions.ResetInputRestrictions();
                _popupLayer = null;
                _popupMovie = null;
                FamilyTree = null;
                IsPopupVisible = false;
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();

            // Cleanup popup if still open
            if (_popupLayer != null)
            {
                ExecuteCloseFamilyTree();
            }
        }
    }
}
