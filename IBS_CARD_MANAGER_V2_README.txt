IBS Card Manager v2 - Sets & Checklists Module

Included:
- Sets management: create, edit, search and delete
- Checklist management: create, edit, search and delete
- CSV checklist import with duplicate update handling
- Inventory checklist lookup by selected set + card number
- Automatic player/team/subset/feature population
- Sidebar links for Sets and Checklists
- EF Core checklist migration using NoAction for Card -> ChecklistItem

After opening in Visual Studio:
1. Build > Rebuild Solution
2. Tools > NuGet Package Manager > Package Manager Console
3. Run: Update-Database
4. Press F5
5. Open Sets from the left menu

CSV required headings:
Card Number, Player

Optional headings:
Team, Subset, Rookie, Autograph, Relic, Refractor, Stock Image URL
