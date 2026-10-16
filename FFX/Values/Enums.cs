namespace Farplane.FFX.Values;

public enum Character
{
    Tidus,
    Yuna,
    Auron,
    Kimahri,
    Wakka,
    Lulu,
    Rikku,
    Seymour,
    Valefor,
    Ifrit,
    Ixion,
    Shiva,
    Bahamut,
    Anima,
    Yojimbo,
    Cindy,
    Sandy,
    Mindy,
    None = 0xFF,
}

public enum DebugFlags
{
    EnemyInvincible = 0x00,           // [mtkmon] Enemies invincible
    PartyInvincible = 0x01,           // [mtkply] Allies invincible
    EnemyControl = 0x02,              // [teki] Control enemies
    EnemyInput = 0x02,                // Legacy alias for EnemyControl
    FreezeCTB = 0x03,                 // [sp0] Freeze CTB turn progression at 1
    GamepadCamera = 0x04,             // [camera] Control camera with gamepad
    FreeCamera = 0x04,                // Legacy alias for GamepadCamera
    CameraStateBackup = 0x05,         // Internal camera state backup
    DisableCameraCuts = 0x06,         // Disable automatic battle camera cuts
    AlignmentPadding = 0x07,          // Unused alignment padding (align 10h)
    DisableAttackAnimation = 0x08,    // [mag0] Disable attack/magic animations
    UnlimitedMp = 0x09,               // [mp0] Unlimited MP / 0 MP cost
    UnlimitedMP = 0x09,               // Legacy alias
    DisableBattleMagicEffects = 0x0A, // [btmag0] Disable battle magic effects
    DisableEventMagicEffects = 0x0B,  // [evmag0] Disable event magic effects
    SkipSummonAnimation = 0x0C,       // [summon] Skip Aeon summon entrance animation
    FullBattleInitUnlock = 0x0D,      // [full] Full battle initialization unlock
    DisableStatusInfliction = 0x0E,   // [stat] Disable all status infliction in battle
    InvertWillDieCheck = 0x0F,        // [willdie] Invert entity Will-Die check
    DisableRandomDamage = 0x10,       // [rnd] Disable random damage variance
    DisableCriticalHit = 0x11,        // [crit] Disable critical hit
    AlwaysHit = 0x12,                 // [abs] Always hit / 100% accuracy
    DebugConsolePrint = 0x13,         // [print] Enable debug log output
    OverdriveAlways = 0x14,           // [limit] Overdrive always available
    AlwaysOverdrive = 0x14,           // Legacy alias
    AlwaysCritical = 0x15,            // [criton] Always critical hit
    AlwaysDeal1 = 0x16,               // Everything hits for 1
    AlwaysDeal10000 = 0x17,           // Everything hits for 10000
    AlwaysDeal99999 = 0x18,           // Everything hits for 99999
    AlwaysRareDrop = 0x19,            // Always rare drop and steal
    BattleAPx100 = 0x1A,              // Battle AP x100
    Ap100X = 0x1A,                    // Legacy alias
    BattleGilx100 = 0x1B,             // Battle Gil x100
    Gil100X = 0x1B,                   // Legacy alias
    DisableOverkill = 0x1C,           // [overkill] Disable overkill
    CameraTargetLook = 0x1D,          // [look] Camera target look mode
    NeverChargeOverdrive = 0x1E,      // [limitoff] Never charge overdrive gauge
    TestParticleEffects = 0x1F,       // [testeff] Test particle effects
    SensorEnabled = 0x20,             // [slow] Battle phase step delay / Sensor flag
    PermanentSensor = 0x20,           // Legacy alias
    BattlePhaseDelayCounter = 0x21,   // Internal 60-frame delay counter for [slow]
    ThrowEjectDebug = 0x22,           // [throw] Throw / Eject debug
    MapDebugFlag = 0x23,              // [map] Battle map debug
    DisplayBattlefieldGrid = 0x24,    // [slowmag] Display battlefield grid / Slow magic
    PlayerAlways1Hp = 0x25,           // [plyhp1] Allies always 1 HP
    PlayerAlways1HP = 0x25,           // Legacy alias
    EnemyAlways1Hp = 0x26,            // [monhp1] Enemies always 1 HP
    EnemyAlways1HP = 0x26,            // Legacy alias
    NegateAbility = 0x27,             // [miss] Always miss / Negate ability
    UnlockAllDevWeaponsInit = 0x28,   // [wep] Load all dev equipment templates on save init
    ScriptItemTestFlag = 0x29,        // [item] ATEL script item test flag
    BattleBehaviour = 0x2A,           // [skip] Skip / Battle behaviour
    ForcedAmbushState = 0x2B,         // Forced ambush/preemptive state (0xFF=Normal, 0=Normal, 1=Preemptive, 2=Ambush)
    PostBattleCampDebug = 0x2C,       // [camp] Post-battle camp debug
    LocalEntityCamera = 0x2D,         // [localcam] Local entity camera
    DisableNeckTracking = 0x2E,       // [neckoff] Disable character neck/head tracking
    CameraFocusDebug = 0x2F,          // [focus] Camera focus debug
    Count = 0x30,                     // Total count of debug flag bytes (48)
}
