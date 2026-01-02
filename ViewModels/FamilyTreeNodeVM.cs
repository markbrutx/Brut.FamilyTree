using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.ViewModelCollection.Encyclopedia.Items;
using TaleWorlds.Library;

namespace Brut.FamilyTree.ViewModels
{
    public class FamilyTreeNodeVM : ViewModel
    {
        private MBBindingList<FamilyTreeNodeVM> _branch = null!;
        private MBBindingList<EncyclopediaFamilyMemberVM> _familyMember = null!;

        public FamilyTreeNodeVM(Hero rootHero, Hero activeHero)
        {
            FamilyBranch = new MBBindingList<FamilyTreeNodeVM>();
            FamilyMember = new MBBindingList<EncyclopediaFamilyMemberVM>
            {
                new EncyclopediaFamilyMemberVM(rootHero, activeHero)
            };

            if (rootHero.Spouse != null)
            {
                FamilyMember.Add(new EncyclopediaFamilyMemberVM(rootHero.Spouse, activeHero));
            }

            foreach (var exSpouse in rootHero.ExSpouses)
            {
                FamilyMember.Add(new EncyclopediaFamilyMemberVM(exSpouse, activeHero));
            }

            foreach (var child in rootHero.Children)
            {
                FamilyBranch.Add(new FamilyTreeNodeVM(child, activeHero));
            }
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            FamilyBranch.ApplyActionOnAllItems(x => x.RefreshValues());
            FamilyMember.ApplyActionOnAllItems(x => x.RefreshValues());
        }

        [DataSourceProperty]
        public MBBindingList<EncyclopediaFamilyMemberVM> FamilyMember
        {
            get => _familyMember;
            set
            {
                if (value != _familyMember)
                {
                    _familyMember = value;
                    OnPropertyChangedWithValue(value, nameof(FamilyMember));
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<FamilyTreeNodeVM> FamilyBranch
        {
            get => _branch;
            set
            {
                if (value != _branch)
                {
                    _branch = value;
                    OnPropertyChangedWithValue(value, nameof(FamilyBranch));
                }
            }
        }
    }
}
