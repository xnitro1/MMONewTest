# Economy System

This directory contains the game's economic systems, including currency, items, shops, and trading mechanics.

## 📁 Directory Structure

### Currency System
- `Currency.cs` - Currency definitions and management

### Item System
- `Item/` - Complete item management system
  - Item types, rarities, and properties
  - Equipment and consumables
  - Crafting materials and recipes

### Shop System
- `CashShop/` - Premium currency shop system
  - `CashShopDatabase.cs` - Shop item management
  - `CashPackage.cs` - Currency packages
  - `Gacha.cs` - Random item systems

## 💰 Economic Features

### Currency Types
- Standard game currency for basic transactions
- Premium currency for special purchases
- Multiple currency support for different economies

### Item Management
- Inventory systems with capacity limits
- Item stacking and organization
- Equipment slots and requirements
- Item durability and repair mechanics

### Shop Systems
- NPC merchants with fixed inventories
- Dynamic pricing based on supply/demand
- Premium cash shops for cosmetic items
- Gacha/luck-based item acquisition

### Trading Mechanics
- Player-to-player trading
- Auction house systems
- Market economies
- Item exchange restrictions

## 🔄 Dependencies

- **Characters**: For inventory ownership and equipment
- **UI**: For shop interfaces and inventory displays
- **Networking**: For synchronized economic transactions
- **Gameplay**: For quest rewards and progression items

## 📊 Economic Balance

### Resource Flow
- **Sources**: Quest rewards, monster drops, crafting, shops
- **Sinks**: Equipment purchases, consumables, repairs
- **Inflation Control**: Item decay, repair costs, premium sinks

### Player Progression
- Early game accessibility through basic items
- Mid-game specialization through equipment choices
- End-game power through rare acquisitions

## 📝 Development Notes

When working with economy systems:
1. Consider inflation and resource sink balance
2. Implement proper validation for transactions
3. Design clear item rarity and progression tiers
4. Test economic systems with large player bases
5. Monitor for exploitable trading loops
6. Consider regional pricing for global games

### Anti-Exploit Measures
- Transaction logging and validation
- Rate limiting for rapid transactions
- Item uniqueness and ownership verification
- Economic anomaly detection
