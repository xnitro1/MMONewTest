#if NET || NETCOREAPP
using Cysharp.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using System.Collections.Generic;

namespace NightBlade.MMO
{
    public partial class PostgreSQLDatabase
    {
        public const string CACHE_KEY_UPSERT_CHARACTER_MOUNT = "UPSERT_CHARACTER_MOUNT";
        private async UniTask FillCharacterRelatesData(TransactionUpdateCharacterState state, NpgsqlConnection connection, NpgsqlTransaction transaction, IPlayerCharacterData characterData, List<CharacterBuff> summonBuffs)
        {
            if (state.Has(TransactionUpdateCharacterState.Attributes))
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_attributes", characterData.Id, characterData.Attributes);
            if (state.Has(TransactionUpdateCharacterState.Buffs))
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_buffs", characterData.Id, characterData.Buffs);
            if (state.Has(TransactionUpdateCharacterState.Hotkeys))
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_hotkeys", characterData.Id, characterData.Hotkeys);
            if (state.Has(TransactionUpdateCharacterState.Items))
            {
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_selectable_weapon_sets", characterData.Id, characterData.SelectableWeaponSets);
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_equip_items", characterData.Id, characterData.EquipItems);
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_non_equip_items", characterData.Id, characterData.NonEquipItems);
            }
            if (state.Has(TransactionUpdateCharacterState.Quests))
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_quests", characterData.Id, characterData.Quests);
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            if (state.Has(TransactionUpdateCharacterState.Currencies))
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_currencies", characterData.Id, characterData.Currencies);
#endif
            if (state.Has(TransactionUpdateCharacterState.Skills))
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_skills", characterData.Id, characterData.Skills);
            if (state.Has(TransactionUpdateCharacterState.SkillUsages))
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_skill_usages", characterData.Id, characterData.SkillUsages);
            if (state.Has(TransactionUpdateCharacterState.Summons))
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_summons", characterData.Id, characterData.Summons);
#if !DISABLE_CUSTOM_CHARACTER_DATA
            if (state.Has(TransactionUpdateCharacterState.ServerCustomData))
            {
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_server_booleans", characterData.Id, characterData.ServerBools);
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_server_int32s", characterData.Id, characterData.ServerInts);
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_server_float32s", characterData.Id, characterData.ServerFloats);
            }
            if (state.Has(TransactionUpdateCharacterState.PrivateCustomData))
            {
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_private_booleans", characterData.Id, characterData.PrivateBools);
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_private_int32s", characterData.Id, characterData.PrivateInts);
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_private_float32s", characterData.Id, characterData.PrivateFloats);
            }
            if (state.Has(TransactionUpdateCharacterState.PublicCustomData))
            {
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_public_booleans", characterData.Id, characterData.PublicBools);
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_public_int32s", characterData.Id, characterData.PublicInts);
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_public_float32s", characterData.Id, characterData.PublicFloats);
            }
#endif

             if (state.Has(TransactionUpdateCharacterState.Mount) && !string.IsNullOrEmpty(characterData.Mount.sourceId))
            {
                await PostgreSQLHelpers.ExecuteUpsert(CACHE_KEY_UPSERT_CHARACTER_MOUNT, connection, transaction, "character_mount", "id",
                    new PostgreSQLHelpers.ColumnInfo("id", characterData.Id),
                    new PostgreSQLHelpers.ColumnInfo("type", (short)characterData.Mount.type),
                    new PostgreSQLHelpers.ColumnInfo("source_id", characterData.Mount.sourceId),
                    new PostgreSQLHelpers.ColumnInfo("mount_remains_duration", characterData.Mount.mountRemainsDuration),
                    new PostgreSQLHelpers.ColumnInfo("level", characterData.Mount.level),
                    new PostgreSQLHelpers.ColumnInfo("current_hp", characterData.Mount.currentHp));
            }
            
#if !DISABLE_CLASSIC_PK
            if (state.Has(TransactionUpdateCharacterState.Pk))
                await UpdateCharacterPk(connection, transaction, characterData);
#endif

            if (summonBuffs != null)
                await PostgreSQLHelpers.ExecuteUpsertJson(connection, transaction, "character_summon_buffs", characterData.Id, summonBuffs);
        }

        public const string CACHE_KEY_CREATE_CHARACTER = "CREATE_CHARACTER";
        public override async UniTask CreateCharacter(string userId, IPlayerCharacterData character)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await PostgreSQLHelpers.ExecuteInsert(
                    CACHE_KEY_CREATE_CHARACTER,
                    connection, transaction,
                    "characters",
                    new PostgreSQLHelpers.ColumnInfo("id", character.Id),
                    new PostgreSQLHelpers.ColumnInfo("user_id", userId),
                    new PostgreSQLHelpers.ColumnInfo("entity_id", character.EntityId),
                    new PostgreSQLHelpers.ColumnInfo("data_id", character.DataId),
                    new PostgreSQLHelpers.ColumnInfo("faction_id", character.FactionId),
                    new PostgreSQLHelpers.ColumnInfo("character_name", character.CharacterName),
                    new PostgreSQLHelpers.ColumnInfo("level", character.Level),
                    new PostgreSQLHelpers.ColumnInfo("exp", character.Exp),
                    new PostgreSQLHelpers.ColumnInfo("current_hp", character.CurrentHp),
                    new PostgreSQLHelpers.ColumnInfo("current_mp", character.CurrentMp),
                    new PostgreSQLHelpers.ColumnInfo("current_stamina", character.CurrentStamina),
                    new PostgreSQLHelpers.ColumnInfo("current_food", character.CurrentFood),
                    new PostgreSQLHelpers.ColumnInfo("current_water", character.CurrentWater),
                    new PostgreSQLHelpers.ColumnInfo("equip_weapon_set", character.EquipWeaponSet),
                    new PostgreSQLHelpers.ColumnInfo("stat_point", character.StatPoint),
                    new PostgreSQLHelpers.ColumnInfo("skill_point", character.SkillPoint),
                    new PostgreSQLHelpers.ColumnInfo("gold", character.Gold),
                    new PostgreSQLHelpers.ColumnInfo("current_channel", character.CurrentChannel),
                    new PostgreSQLHelpers.ColumnInfo("current_map_name", character.CurrentMapName),
                    new PostgreSQLHelpers.ColumnInfo("current_position_x", character.CurrentPosition.x),
                    new PostgreSQLHelpers.ColumnInfo("current_position_y", character.CurrentPosition.y),
                    new PostgreSQLHelpers.ColumnInfo("current_position_z", character.CurrentPosition.z),
                    new PostgreSQLHelpers.ColumnInfo("current_rotation_x", character.CurrentRotation.x),
                    new PostgreSQLHelpers.ColumnInfo("current_rotation_y", character.CurrentRotation.y),
                    new PostgreSQLHelpers.ColumnInfo("current_rotation_z", character.CurrentRotation.z),
                    new PostgreSQLHelpers.ColumnInfo("current_safe_area", character.CurrentSafeArea),
#if !DISABLE_DIFFER_MAP_RESPAWNING
                    new PostgreSQLHelpers.ColumnInfo("respawn_map_name", character.RespawnMapName),
                    new PostgreSQLHelpers.ColumnInfo("respawn_position_x", character.RespawnPosition.x),
                    new PostgreSQLHelpers.ColumnInfo("respawn_position_y", character.RespawnPosition.y),
                    new PostgreSQLHelpers.ColumnInfo("respawn_position_z", character.RespawnPosition.z),
#endif
                    new PostgreSQLHelpers.ColumnInfo("reputation", character.Reputation));
                TransactionUpdateCharacterState state = TransactionUpdateCharacterState.All;
                await FillCharacterRelatesData(state, connection, transaction, character, null);
                if (onCreateCharacter != null)
                    onCreateCharacter.Invoke(connection, transaction, userId, character);
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, $"Transaction, Error occurs while create character: {character.Id}");
                LogException(LogTag, ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        private bool GetCharacter(NpgsqlDataReader reader, out PlayerCharacterData character)
        {
            if (reader.Read())
            {
                character = new PlayerCharacterData();
                character.Id = reader.GetString(0);
                character.UserId = reader.GetString(1);
                character.DataId = reader.GetInt32(2);
                character.EntityId = reader.GetInt32(3);
                character.FactionId = reader.GetInt32(4);
                character.CharacterName = reader.GetString(5);
                character.Level = reader.GetInt32(6);
                character.Exp = reader.GetInt32(7);
                character.CurrentHp = reader.GetInt32(8);
                character.CurrentMp = reader.GetInt32(9);
                character.CurrentStamina = reader.GetInt32(10);
                character.CurrentFood = reader.GetInt32(11);
                character.CurrentWater = reader.GetInt32(12);
                character.EquipWeaponSet = reader.GetByte(13);
                character.StatPoint = reader.GetFloat(14);
                character.SkillPoint = reader.GetFloat(15);
                character.Gold = reader.GetInt32(16);
                character.PartyId = reader.GetInt32(17);
                character.GuildId = reader.GetInt32(18);
                character.GuildRole = reader.GetByte(19);
                character.CurrentChannel = reader.GetString(20);
                character.CurrentMapName = reader.GetString(21);
                character.CurrentPosition = new Vec3(reader.GetFloat(22), reader.GetFloat(23), reader.GetFloat(24));
                character.CurrentRotation = new Vec3(reader.GetFloat(25), reader.GetFloat(26), reader.GetFloat(27));
                character.CurrentSafeArea = reader.GetString(28);
#if !DISABLE_DIFFER_MAP_RESPAWNING
                character.RespawnMapName = reader.GetString(29);
                character.RespawnPosition = new Vec3(reader.GetFloat(30), reader.GetFloat(31), reader.GetFloat(32));
#endif
                character.Reputation = reader.GetInt32(33);
                character.LastDeadTime = reader.GetInt64(34);
                character.UnmuteTime = reader.GetInt64(35);
                character.LastUpdate = ((System.DateTimeOffset)System.DateTime.SpecifyKind(reader.GetDateTime(36), System.DateTimeKind.Utc)).ToUnixTimeSeconds();
#if !DISABLE_CLASSIC_PK
                if (!reader.IsDBNull(37))
                    character.IsPkOn = reader.GetBoolean(37);
                if (!reader.IsDBNull(38))
                    character.LastPkOnTime = reader.GetInt64(38);
                if (!reader.IsDBNull(39))
                    character.PkPoint = reader.GetInt32(39);
                if (!reader.IsDBNull(40))
                    character.ConsecutivePkKills = reader.GetInt32(40);
                if (!reader.IsDBNull(41))
                    character.HighestPkPoint = reader.GetInt32(41);
                if (!reader.IsDBNull(42))
                    character.HighestConsecutivePkKills = reader.GetInt32(42);
#endif
                CharacterMount mount = new CharacterMount();
                if (!reader.IsDBNull(43))
                    mount.type = (MountType)reader.GetInt16(43);
                if (!reader.IsDBNull(44))
                    mount.sourceId = reader.GetString(44);
                if (!reader.IsDBNull(45))
                    mount.mountRemainsDuration = reader.GetFloat(45);
                if (!reader.IsDBNull(46))
                    mount.level = reader.GetInt32(46);
                if (!reader.IsDBNull(47))
                    mount.currentHp = reader.GetInt32(47);
                character.Mount = mount;
                return true;
            }
            character = null;
            return false;
        }

        public override async UniTask<PlayerCharacterData> GetCharacter(
            string id,
            bool withEquipWeapons = true,
            bool withAttributes = true,
            bool withSkills = true,
            bool withSkillUsages = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withSummons = true,
            bool withHotkeys = true,
            bool withQuests = true,
            bool withCurrencies = true,
            bool withServerCustomData = true,
            bool withPrivateCustomData = true,
            bool withPublicCustomData = true)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            return await GetCharacter(
                connection,
                id,
                withEquipWeapons,
                withAttributes,
                withSkills,
                withSkillUsages,
                withBuffs,
                withEquipItems,
                withNonEquipItems,
                withSummons,
                withHotkeys,
                withQuests,
                withCurrencies,
                withServerCustomData,
                withPrivateCustomData,
                withPublicCustomData);
        }

        public async UniTask<PlayerCharacterData> GetCharacter(
            NpgsqlConnection connection,
            string id,
            bool withEquipWeapons,
            bool withAttributes,
            bool withSkills,
            bool withSkillUsages,
            bool withBuffs,
            bool withEquipItems,
            bool withNonEquipItems,
            bool withSummons,
            bool withHotkeys,
            bool withQuests,
            bool withCurrencies,
            bool withServerCustomData,
            bool withPrivateCustomData,
            bool withPublicCustomData)
        {
            PlayerCharacterData result;
            NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT
                c.id, c.user_id, c.data_id, c.entity_id, c.faction_id, c.character_name, c.level, c.exp,
                c.current_hp, c.current_mp, c.current_stamina, c.current_food, c.current_water,
                c.equip_weapon_set, c.stat_point, c.skill_point, c.gold, c.party_id, c.guild_id, c.guild_role,
                c.current_channel,
                c.current_map_name, c.current_position_x, c.current_position_y, c.current_position_z, c.current_rotation_x, c.current_rotation_y, c.current_rotation_z,
                c.current_safe_area,
                c.respawn_map_name, c.respawn_position_x, c.respawn_position_y, c.respawn_position_z,
                c.reputation, c.last_dead_time, c.unmute_time, c.update_time,
                cpk.is_pk_on, cpk.last_pk_on_time, cpk.pk_point, cpk.consecutive_pk_kills, cpk.highest_pk_point, cpk.highest_consecutive_pk_kills,
                cmnt.type, cmnt.source_id, cmnt.mount_remains_duration, cmnt.level, cmnt.current_hp
                FROM characters AS c 
                LEFT JOIN character_pk AS cpk ON c.id = cpk.id
                LEFT JOIN character_mount AS cmnt ON c.id = cmnt.id
                WHERE c.id=$1 LIMIT 1", connection);
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Varchar });
            await cmd.PrepareAsync();
            cmd.Parameters[0].Value = id;
            var reader = await cmd.ExecuteReaderAsync();
            if (!GetCharacter(reader, out result))
            {
                reader.Dispose();
                cmd.Dispose();
                return result;
            }
            reader.Dispose();
            cmd.Dispose();

            // Found character, then read its relates data in parallel
            // Create task lists for parallel execution
            List<UniTask> tasks = new List<UniTask>();
            
            // Temporary holders for results
            EquipWeapons[] selectableWeaponSets = null;
            CharacterAttribute[] attributes = null;
            CharacterSkill[] skills = null;
            CharacterSkillUsage[] skillUsages = null;
            CharacterBuff[] buffs = null;
            CharacterItem[] equipItems = null;
            CharacterItem[] nonEquipItems = null;
            CharacterSummon[] summons = null;
            CharacterHotkey[] hotkeys = null;
            CharacterQuest[] quests = null;
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            CharacterCurrency[] currencies = null;
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
            CharacterDataBoolean[] serverBools = null;
            CharacterDataInt32[] serverInts = null;
            CharacterDataFloat32[] serverFloats = null;
            CharacterDataBoolean[] privateBools = null;
            CharacterDataInt32[] privateInts = null;
            CharacterDataFloat32[] privateFloats = null;
            CharacterDataBoolean[] publicBools = null;
            CharacterDataInt32[] publicInts = null;
            CharacterDataFloat32[] publicFloats = null;
#endif

            // Add parallel tasks
            if (withEquipWeapons)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<EquipWeapons[]>(connection, "character_selectable_weapon_sets", id).ContinueWith(r => selectableWeaponSets = r));
            if (withAttributes)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterAttribute[]>(connection, "character_attributes", id).ContinueWith(r => attributes = r));
            if (withSkills)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterSkill[]>(connection, "character_skills", id).ContinueWith(r => skills = r));
            if (withSkillUsages)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterSkillUsage[]>(connection, "character_skill_usages", id).ContinueWith(r => skillUsages = r));
            if (withBuffs)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterBuff[]>(connection, "character_buffs", id).ContinueWith(r => buffs = r));
            if (withEquipItems)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterItem[]>(connection, "character_equip_items", id).ContinueWith(r => equipItems = r));
            if (withNonEquipItems)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterItem[]>(connection, "character_non_equip_items", id).ContinueWith(r => nonEquipItems = r));
            if (withSummons)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterSummon[]>(connection, "character_summons", id).ContinueWith(r => summons = r));
            if (withHotkeys)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterHotkey[]>(connection, "character_hotkeys", id).ContinueWith(r => hotkeys = r));
            if (withQuests)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterQuest[]>(connection, "character_quests", id).ContinueWith(r => quests = r));
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            if (withCurrencies)
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterCurrency[]>(connection, "character_currencies", id).ContinueWith(r => currencies = r));
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
            if (withServerCustomData)
            {
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterDataBoolean[]>(connection, "character_server_booleans", id).ContinueWith(r => serverBools = r));
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterDataInt32[]>(connection, "character_server_int32s", id).ContinueWith(r => serverInts = r));
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterDataFloat32[]>(connection, "character_server_float32s", id).ContinueWith(r => serverFloats = r));
            }
            if (withPrivateCustomData)
            {
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterDataBoolean[]>(connection, "character_private_booleans", id).ContinueWith(r => privateBools = r));
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterDataInt32[]>(connection, "character_private_int32s", id).ContinueWith(r => privateInts = r));
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterDataFloat32[]>(connection, "character_private_float32s", id).ContinueWith(r => privateFloats = r));
            }
            if (withPublicCustomData)
            {
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterDataBoolean[]>(connection, "character_public_booleans", id).ContinueWith(r => publicBools = r));
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterDataInt32[]>(connection, "character_public_int32s", id).ContinueWith(r => publicInts = r));
                tasks.Add(PostgreSQLHelpers.ExecuteSelectJson<CharacterDataFloat32[]>(connection, "character_public_float32s", id).ContinueWith(r => publicFloats = r));
            }
#endif

            // Execute all queries in parallel
            await UniTask.WhenAll(tasks);

            // Assign results
            if (withEquipWeapons && selectableWeaponSets != null)
                result.SelectableWeaponSets = new List<EquipWeapons>(selectableWeaponSets);
            if (withAttributes && attributes != null)
                result.Attributes = new List<CharacterAttribute>(attributes);
            if (withSkills && skills != null)
                result.Skills = new List<CharacterSkill>(skills);
            if (withSkillUsages && skillUsages != null)
                result.SkillUsages = new List<CharacterSkillUsage>(skillUsages);
            if (withBuffs && buffs != null)
                result.Buffs = new List<CharacterBuff>(buffs);
            if (withEquipItems && equipItems != null)
                result.EquipItems = new List<CharacterItem>(equipItems);
            if (withNonEquipItems && nonEquipItems != null)
                result.NonEquipItems = new List<CharacterItem>(nonEquipItems);
            if (withSummons && summons != null)
                result.Summons = new List<CharacterSummon>(summons);
            if (withHotkeys && hotkeys != null)
                result.Hotkeys = new List<CharacterHotkey>(hotkeys);
            if (withQuests && quests != null)
                result.Quests = new List<CharacterQuest>(quests);
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            if (withCurrencies && currencies != null)
                result.Currencies = new List<CharacterCurrency>(currencies);
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
            if (withServerCustomData)
            {
                if (serverBools != null) result.ServerBools = new List<CharacterDataBoolean>(serverBools);
                if (serverInts != null) result.ServerInts = new List<CharacterDataInt32>(serverInts);
                if (serverFloats != null) result.ServerFloats = new List<CharacterDataFloat32>(serverFloats);
            }
            if (withPrivateCustomData)
            {
                if (privateBools != null) result.PrivateBools = new List<CharacterDataBoolean>(privateBools);
                if (privateInts != null) result.PrivateInts = new List<CharacterDataInt32>(privateInts);
                if (privateFloats != null) result.PrivateFloats = new List<CharacterDataFloat32>(privateFloats);
            }
            if (withPublicCustomData)
            {
                if (publicBools != null) result.PublicBools = new List<CharacterDataBoolean>(publicBools);
                if (publicInts != null) result.PublicInts = new List<CharacterDataInt32>(publicInts);
                if (publicFloats != null) result.PublicFloats = new List<CharacterDataFloat32>(publicFloats);
            }
#endif
            if (onGetCharacter != null)
            {
                result = onGetCharacter.Invoke(
                    result,
                    withEquipWeapons,
                    withAttributes,
                    withSkills,
                    withSkillUsages,
                    withBuffs,
                    withEquipItems,
                    withNonEquipItems,
                    withSummons,
                    withHotkeys,
                    withQuests,
                    withCurrencies,
                    withServerCustomData,
                    withPrivateCustomData,
                    withPublicCustomData);
            }
            return result;
        }

        public const string CACHE_KEY_GET_CHARACTERS = "GET_CHARACTERS";
        public override async UniTask<List<PlayerCharacterData>> GetCharacters(string userId)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            List<PlayerCharacterData> result = new List<PlayerCharacterData>();
            Dictionary<string, PlayerCharacterData> characterDict = new Dictionary<string, PlayerCharacterData>();

            // Single query to get all characters with base data + mount + pk info
            NpgsqlCommand cmd = new NpgsqlCommand(@"SELECT
                c.id, c.user_id, c.data_id, c.entity_id, c.faction_id, c.character_name, c.level, c.exp,
                c.current_hp, c.current_mp, c.current_stamina, c.current_food, c.current_water,
                c.equip_weapon_set, c.stat_point, c.skill_point, c.gold, c.party_id, c.guild_id, c.guild_role,
                c.current_channel,
                c.current_map_name, c.current_position_x, c.current_position_y, c.current_position_z, c.current_rotation_x, c.current_rotation_y, c.current_rotation_z,
                c.current_safe_area,
                c.respawn_map_name, c.respawn_position_x, c.respawn_position_y, c.respawn_position_z,
                c.icon_data_id, c.frame_data_id, c.title_data_id, c.reputation, c.last_dead_time, c.unmute_time, c.update_time,
                cpk.is_pk_on, cpk.last_pk_on_time, cpk.pk_point, cpk.consecutive_pk_kills, cpk.highest_pk_point, cpk.highest_consecutive_pk_kills,
                cmnt.type, cmnt.source_id, cmnt.mount_remains_duration, cmnt.level, cmnt.current_hp
                FROM characters AS c 
                LEFT JOIN character_pk AS cpk ON c.id = cpk.id
                LEFT JOIN character_mount AS cmnt ON c.id = cmnt.id
                WHERE c.user_id=$1 ORDER BY c.update_time DESC", connection);
            cmd.Parameters.Add(new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Varchar });
            await cmd.PrepareAsync();
            cmd.Parameters[0].Value = userId;
            var reader = await cmd.ExecuteReaderAsync();
            
            PlayerCharacterData character;
            while (GetCharacter(reader, out character))
            {
                result.Add(character);
                characterDict[character.Id] = character;
            }
            reader.Dispose();
            cmd.Dispose();

            if (result.Count == 0)
                return result;

            // Batch load related data for all characters in parallel
            List<string> characterIds = new List<string>(characterDict.Keys);

            // For character list, we need: equip weapons, attributes, skills, equip items, and public custom data
            List<UniTask> tasks = new List<UniTask>();
            tasks.Add(ReadCharacterSelectableWeaponSetsBatch(connection, characterIds, characterDict));
            tasks.Add(ReadCharacterAttributesBatch(connection, characterIds, characterDict));
            tasks.Add(ReadCharacterSkillsBatch(connection, characterIds, characterDict));
            tasks.Add(ReadCharacterEquipItemsBatch(connection, characterIds, characterDict));
#if !DISABLE_CUSTOM_CHARACTER_DATA
            tasks.Add(ReadCharacterPublicDataBatch(connection, characterIds, characterDict));
#endif
            await UniTask.WhenAll(tasks);

            return result;
        }

        private async UniTask ReadCharacterSelectableWeaponSetsBatch(NpgsqlConnection connection, List<string> characterIds, Dictionary<string, PlayerCharacterData> characterDict)
        {
            if (characterIds.Count == 0) return;

            foreach (var kvp in characterDict)
            {
                var data = await PostgreSQLHelpers.ExecuteSelectJson<EquipWeapons[]>(connection, "character_selectable_weapon_sets", kvp.Key);
                if (data != null)
                    kvp.Value.SelectableWeaponSets = new List<EquipWeapons>(data);
            }
        }

        private async UniTask ReadCharacterAttributesBatch(NpgsqlConnection connection, List<string> characterIds, Dictionary<string, PlayerCharacterData> characterDict)
        {
            if (characterIds.Count == 0) return;

            foreach (var kvp in characterDict)
            {
                var data = await PostgreSQLHelpers.ExecuteSelectJson<CharacterAttribute[]>(connection, "character_attributes", kvp.Key);
                if (data != null)
                    kvp.Value.Attributes = new List<CharacterAttribute>(data);
            }
        }

        private async UniTask ReadCharacterSkillsBatch(NpgsqlConnection connection, List<string> characterIds, Dictionary<string, PlayerCharacterData> characterDict)
        {
            if (characterIds.Count == 0) return;

            foreach (var kvp in characterDict)
            {
                var data = await PostgreSQLHelpers.ExecuteSelectJson<CharacterSkill[]>(connection, "character_skills", kvp.Key);
                if (data != null)
                    kvp.Value.Skills = new List<CharacterSkill>(data);
            }
        }

        private async UniTask ReadCharacterEquipItemsBatch(NpgsqlConnection connection, List<string> characterIds, Dictionary<string, PlayerCharacterData> characterDict)
        {
            if (characterIds.Count == 0) return;

            foreach (var kvp in characterDict)
            {
                var data = await PostgreSQLHelpers.ExecuteSelectJson<CharacterItem[]>(connection, "character_equip_items", kvp.Key);
                if (data != null)
                    kvp.Value.EquipItems = new List<CharacterItem>(data);
            }
        }

#if !DISABLE_CUSTOM_CHARACTER_DATA
        private async UniTask ReadCharacterPublicDataBatch(NpgsqlConnection connection, List<string> characterIds, Dictionary<string, PlayerCharacterData> characterDict)
        {
            if (characterIds.Count == 0) return;

            // Load public data for each character
            foreach (var kvp in characterDict)
            {
                var bools = await PostgreSQLHelpers.ExecuteSelectJson<CharacterDataBoolean[]>(connection, "character_public_booleans", kvp.Key);
                if (bools != null)
                    kvp.Value.PublicBools = new List<CharacterDataBoolean>(bools);

                var ints = await PostgreSQLHelpers.ExecuteSelectJson<CharacterDataInt32[]>(connection, "character_public_int32s", kvp.Key);
                if (ints != null)
                    kvp.Value.PublicInts = new List<CharacterDataInt32>(ints);

                var floats = await PostgreSQLHelpers.ExecuteSelectJson<CharacterDataFloat32[]>(connection, "character_public_float32s", kvp.Key);
                if (floats != null)
                    kvp.Value.PublicFloats = new List<CharacterDataFloat32>(floats);
            }
        }
#endif

        public const string CACHE_KEY_UPDATE_CHARACTER_PK = "UPDATE_CHARACTER_PK";
        public async UniTask UpdateCharacterPk(NpgsqlConnection connection, NpgsqlTransaction transaction, IPlayerCharacterData character)
        {
#if !DISABLE_CLASSIC_PK
            await PostgreSQLHelpers.ExecuteUpsert(
                CACHE_KEY_UPDATE_CHARACTER_PK,
                connection, transaction,
                "character_pk",
                "id",
                new PostgreSQLHelpers.ColumnInfo("id", character.Id),
                new PostgreSQLHelpers.ColumnInfo("is_pk_on", character.IsPkOn),
                new PostgreSQLHelpers.ColumnInfo("last_pk_on_time", character.LastPkOnTime),
                new PostgreSQLHelpers.ColumnInfo("pk_point", character.PkPoint),
                new PostgreSQLHelpers.ColumnInfo("consecutive_pk_kills", character.ConsecutivePkKills),
                new PostgreSQLHelpers.ColumnInfo("highest_pk_point", character.HighestPkPoint),
                new PostgreSQLHelpers.ColumnInfo("highest_consecutive_pk_kills", character.HighestConsecutivePkKills));
#else
            await UniTask.Yield();
#endif
        }

        public const string CACHE_KEY_UPDATE_CHARACTER = "UPDATE_CHARACTER";
        public override async UniTask UpdateCharacter(TransactionUpdateCharacterState state, IPlayerCharacterData character, List<CharacterBuff> summonBuffs, bool deleteStorageReservation)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                if (state.Has(TransactionUpdateCharacterState.Character))
                {
                    await PostgreSQLHelpers.ExecuteUpdate(
                        CACHE_KEY_UPDATE_CHARACTER,
                        connection, transaction,
                        "characters",
                        new[]
                        {
                            new PostgreSQLHelpers.ColumnInfo("entity_id", character.EntityId),
                            new PostgreSQLHelpers.ColumnInfo("data_id", character.DataId),
                            new PostgreSQLHelpers.ColumnInfo("faction_id", character.FactionId),
                            new PostgreSQLHelpers.ColumnInfo("character_name", character.CharacterName),
                            new PostgreSQLHelpers.ColumnInfo("level", character.Level),
                            new PostgreSQLHelpers.ColumnInfo("exp", character.Exp),
                            new PostgreSQLHelpers.ColumnInfo("current_hp", character.CurrentHp),
                            new PostgreSQLHelpers.ColumnInfo("current_mp", character.CurrentMp),
                            new PostgreSQLHelpers.ColumnInfo("current_stamina", character.CurrentStamina),
                            new PostgreSQLHelpers.ColumnInfo("current_food", character.CurrentFood),
                            new PostgreSQLHelpers.ColumnInfo("current_water", character.CurrentWater),
                            new PostgreSQLHelpers.ColumnInfo("equip_weapon_set", character.EquipWeaponSet),
                            new PostgreSQLHelpers.ColumnInfo("stat_point", character.StatPoint),
                            new PostgreSQLHelpers.ColumnInfo("skill_point", character.SkillPoint),
                            new PostgreSQLHelpers.ColumnInfo("gold", character.Gold),
                            new PostgreSQLHelpers.ColumnInfo("current_channel", character.CurrentChannel),
                            new PostgreSQLHelpers.ColumnInfo("current_map_name", character.CurrentMapName),
                            new PostgreSQLHelpers.ColumnInfo("current_position_x", character.CurrentPosition.x),
                            new PostgreSQLHelpers.ColumnInfo("current_position_y", character.CurrentPosition.y),
                            new PostgreSQLHelpers.ColumnInfo("current_position_z", character.CurrentPosition.z),
                            new PostgreSQLHelpers.ColumnInfo("current_rotation_x", character.CurrentRotation.x),
                            new PostgreSQLHelpers.ColumnInfo("current_rotation_y", character.CurrentRotation.y),
                            new PostgreSQLHelpers.ColumnInfo("current_rotation_z", character.CurrentRotation.z),
                            new PostgreSQLHelpers.ColumnInfo("current_safe_area", character.CurrentSafeArea),
#if !DISABLE_DIFFER_MAP_RESPAWNING
                            new PostgreSQLHelpers.ColumnInfo("respawn_map_name", character.RespawnMapName),
                            new PostgreSQLHelpers.ColumnInfo("respawn_position_x", character.RespawnPosition.x),
                            new PostgreSQLHelpers.ColumnInfo("respawn_position_y", character.RespawnPosition.y),
                            new PostgreSQLHelpers.ColumnInfo("respawn_position_z", character.RespawnPosition.z),
#endif
                            new PostgreSQLHelpers.ColumnInfo("reputation", character.Reputation),
                            new PostgreSQLHelpers.ColumnInfo("last_dead_time", character.LastDeadTime),
                            new PostgreSQLHelpers.ColumnInfo("unmute_time", character.UnmuteTime),
                            new PostgreSQLHelpers.ColumnInfo(NpgsqlDbType.TimestampTz, "update_time", "timezone('utc', now())"),
                        },
                        PostgreSQLHelpers.WhereEqualTo("id", character.Id));
                }
                await FillCharacterRelatesData(state, connection, transaction, character, summonBuffs);
                if (deleteStorageReservation)
                {
                    await DeleteReservedStorageByReserver(character.Id);
                }
                if (onUpdateCharacter != null)
                    onUpdateCharacter.Invoke(connection, transaction, state, character);
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, $"Transaction, Error occurs while update character: {character.Id}");
                LogException(LogTag, ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public override async UniTask DeleteCharacter(string userId, string id)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            using var transaction = await connection.BeginTransactionAsync();
            try
            {
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_attributes", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_buffs", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_currencies", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_hotkeys", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_selectable_weapon_sets", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_equip_items", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_non_equip_items", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_quests", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_skills", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_skill_usages", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_summons", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_mount", id);

                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_server_booleans", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_server_int32s", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_server_float32s", id);

                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_private_booleans", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_private_int32s", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_private_float32s", id);

                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_public_booleans", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_public_int32s", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_public_float32s", id);

                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "characters", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "character_pk", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "friends", "character_id_1", id);
                await PostgreSQLHelpers.ExecuteDeleteById(connection, transaction, "friends", "character_id_2", id);

                if (onDeleteCharacter != null)
                    onDeleteCharacter.Invoke(connection, transaction, userId, id);
                await transaction.CommitAsync();
            }
            catch (System.Exception ex)
            {
                LogError(LogTag, $"Transaction, Error occurs while deleting character: {id}");
                LogException(LogTag, ex);
                await transaction.RollbackAsync();
                throw;
            }
        }

        public const string CACHE_KEY_FIND_CHARACTER_NAME = "FIND_CHARACTER_NAME";
        public override async UniTask<long> FindCharacterName(string characterName)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            return await PostgreSQLHelpers.ExecuteCount(
                CACHE_KEY_FIND_CHARACTER_NAME,
                connection,
                "characters",
                PostgreSQLHelpers.WhereLike("LOWER(character_name)", characterName.ToLower()));
        }

        public const string CACHE_KEY_GET_ID_BY_CHARACTER_NAME = "GET_ID_BY_CHARACTER_NAME";
        public override async UniTask<string> GetIdByCharacterName(string characterName)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            object result = PostgreSQLHelpers.ExecuteSelectScalar(
                CACHE_KEY_GET_ID_BY_CHARACTER_NAME,
                connection,
                "characters", "id", "LIMIT 1",
                PostgreSQLHelpers.WhereEqualTo("character_name", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public const string CACHE_KEY_GET_USER_ID_BY_CHARACTER_NAME = "GET_USER_ID_BY_CHARACTER_NAME";
        public override async UniTask<string> GetUserIdByCharacterName(string characterName)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            object result = PostgreSQLHelpers.ExecuteSelectScalar(
                CACHE_KEY_GET_USER_ID_BY_CHARACTER_NAME,
                connection,
                "characters", "user_id", "LIMIT 1",
                PostgreSQLHelpers.WhereEqualTo("character_name", characterName));
            return result != null ? (string)result : string.Empty;
        }

        public const string CACHE_KEY_FIND_CHARACTERS_SELECT_FRIENDS = "FIND_CHARACTERS_SELECT_FRIENDS";
        public override async UniTask<List<SocialCharacterData>> FindCharacters(string finderId, string characterName, int skip, int limit)
        {
            if (limit <= 0)
                limit = 25;
            using var connection = await _dataSource.OpenConnectionAsync();
            
            // Collect friend IDs to exclude using parameterized query (SQL injection fix)
            List<string> excludeIds = new List<string> { finderId };
            var readerIds = await PostgreSQLHelpers.ExecuteSelect(
                CACHE_KEY_FIND_CHARACTERS_SELECT_FRIENDS,
                connection,
                "friends", "character_id_2",
                PostgreSQLHelpers.WhereEqualTo("character_id_1", finderId));
            while (readerIds.Read())
            {
                excludeIds.Add(readerIds.GetString(0));
            }
            readerIds.Dispose();

            // Build parameterized WHERE clauses for exclusion
            List<PostgreSQLHelpers.WhereQuery> whereQueries = new List<PostgreSQLHelpers.WhereQuery>();
            whereQueries.Add(PostgreSQLHelpers.WhereLike("LOWER(character_name)", $"%{characterName.ToLower()}%"));
            
            // Add exclusion queries using parameterized methods
            for (int i = 0; i < excludeIds.Count; i++)
            {
                whereQueries.Add(PostgreSQLHelpers.AndWhereNotEqualTo("id", excludeIds[i]));
            }

            // Read some character data with parameterized query
            using var readerCharacters = await PostgreSQLHelpers.ExecuteSelect(
                null,
                connection,
                "characters", "id, data_id, character_name, level",
                $"ORDER BY RANDOM() OFFSET {skip} LIMIT {limit}",
                whereQueries.ToArray());
            List<SocialCharacterData> characters = new List<SocialCharacterData>();
            SocialCharacterData tempCharacter;
            while (readerCharacters.Read())
            {
                tempCharacter = new SocialCharacterData();
                tempCharacter.id = readerCharacters.GetString(0);
                tempCharacter.dataId = readerCharacters.GetInt32(1);
                tempCharacter.characterName = readerCharacters.GetString(2);
                tempCharacter.level = readerCharacters.GetInt32(3);
                characters.Add(tempCharacter);
            }
            return characters;
        }

        public const string CACHE_KEY_CREATE_FRIEND = "CREATE_FRIEND";
        public override async UniTask CreateFriend(string id1, string id2, byte state)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            await PostgreSQLHelpers.ExecuteUpsert(
                CACHE_KEY_CREATE_FRIEND,
                connection, null,
                "friends",
                "character_id_1, character_id_2",
                new PostgreSQLHelpers.ColumnInfo("state", state),
                new PostgreSQLHelpers.ColumnInfo("character_id_1", id1),
                new PostgreSQLHelpers.ColumnInfo("character_id_2", id2));
        }

        public const string CACHE_KEY_DELETE_FRIEND = "DELETE_FRIEND";
        public override async UniTask DeleteFriend(string id1, string id2)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            await PostgreSQLHelpers.ExecuteDelete(
                CACHE_KEY_DELETE_FRIEND,
                connection, null,
                "friends",
                PostgreSQLHelpers.WhereEqualTo("character_id_1", id1),
                PostgreSQLHelpers.AndWhereEqualTo("character_id_2", id2));
        }

        public const string CACHE_KEY_GET_FRIENDS_ID_1 = "GET_FRIENDS_ID_1";
        public const string CACHE_KEY_GET_FRIENDS_ID_2 = "GET_FRIENDS_ID_2";
        public override async UniTask<List<SocialCharacterData>> GetFriends(string id, bool readById2, byte state, int skip, int limit)
        {
            if (limit <= 0)
                limit = 25;
            using var connection = await _dataSource.OpenConnectionAsync();
            List<string> characterIds = new List<string>();
            if (readById2)
            {
                var readerIds = await PostgreSQLHelpers.ExecuteSelect(
                    CACHE_KEY_GET_FRIENDS_ID_1,
                    connection,
                    "friends", "character_id_1", $"OFFSET {skip} LIMIT {limit}",
                    PostgreSQLHelpers.WhereEqualTo("character_id_2", id),
                    PostgreSQLHelpers.AndWhereSmallEqualTo("state", state));
                while (readerIds.Read())
                {
                    characterIds.Add(readerIds.GetString(0));
                }
                readerIds.Dispose();
            }
            else
            {
                var readerIds = await PostgreSQLHelpers.ExecuteSelect(
                    CACHE_KEY_GET_FRIENDS_ID_2,
                    connection,
                    "friends", "character_id_2", $"OFFSET {skip} LIMIT {limit}",
                    PostgreSQLHelpers.WhereEqualTo("character_id_1", id),
                    PostgreSQLHelpers.AndWhereSmallEqualTo("state", state));
                while (readerIds.Read())
                {
                    characterIds.Add(readerIds.GetString(0));
                }
                readerIds.Dispose();
            }
            return await GetSocialCharacterByIds(connection, characterIds);
        }

        public const string CACHE_KEY_GET_FRIEND_REQUESTS_NOTIFICATION = "GET_FRIEND_REQUESTS_NOTIFICATION";
        public override async UniTask<int> GetFriendRequestNotification(string characterId)
        {
            using var connection = await _dataSource.OpenConnectionAsync();
            return (int)await PostgreSQLHelpers.ExecuteCount(
                CACHE_KEY_GET_FRIEND_REQUESTS_NOTIFICATION,
                connection,
                "friends",
                PostgreSQLHelpers.WhereEqualTo("character_id_2", characterId),
                PostgreSQLHelpers.AndWhereSmallEqualTo("state", 1));
        }

        public async UniTask<List<SocialCharacterData>> GetSocialCharacterByIds(NpgsqlConnection connection, IList<string> characterIds)
        {
            List<SocialCharacterData> characters = new List<SocialCharacterData>();
            if (characterIds.Count > 0)
            {
                List<PostgreSQLHelpers.WhereQuery> characterQueries = new List<PostgreSQLHelpers.WhereQuery>()
                {
                    PostgreSQLHelpers.WhereEqualTo("id", characterIds[0]),
                };
                for (int i = 1; i < characterIds.Count; ++i)
                {
                    characterQueries.Add(PostgreSQLHelpers.OrWhereEqualTo("id", characterIds[i]));
                }
                using var readerCharacters = await PostgreSQLHelpers.ExecuteSelect(
                    null,
                    connection,
                    "characters",
                    characterQueries,
                    "id, data_id, character_name, level");
                SocialCharacterData tempCharacter;
                while (readerCharacters.Read())
                {
                    tempCharacter = new SocialCharacterData();
                    tempCharacter.id = readerCharacters.GetString(0);
                    tempCharacter.dataId = readerCharacters.GetInt32(1);
                    tempCharacter.characterName = readerCharacters.GetString(2);
                    tempCharacter.level = readerCharacters.GetInt32(3);
                    characters.Add(tempCharacter);
                }
            }
            return characters;
        }
    }
}
#endif







