using JE_longint = System.Int32;
using JE_integer = System.Int16;
using JE_shortint = System.SByte;
using JE_word = System.UInt16;
using JE_byte = System.Byte;
using JE_boolean = System.Boolean;
using JE_char = System.Char;
using JE_real = System.Single;

using static ConfigC;

public static class PlayerC
{
    public const int FRONT_WEAPON = 0;
    public const int REAR_WEAPON = 1;

    public const int LEFT_SIDEKICK = 0;
    public const int RIGHT_SIDEKICK = 1;

    public class PlayerItems
    {
        public JE_word ship;
        public JE_word generator;
        public JE_word shield;
        public struct Weapon
        {
            public JE_word id;
            public JE_word power;
        }
        public Weapon[] weapon = { new Weapon(), new Weapon() };
        public JE_word[] sidekick = new JE_word[2];
        public JE_word special;

        // Dragonwing only:
        // repeatedly collecting the same powerup gives a series of sidekick upgrades
        public JE_word sidekick_series;
        public JE_word sidekick_level;

        // Single-player only
        public JE_word super_arcade_mode;  // stored as an item for compatibility :(

        public PlayerItems Clone()
        {
            PlayerItems ret = (PlayerItems)MemberwiseClone();
            sidekick = new JE_word[] { sidekick[0], sidekick[1] };
            weapon = new Weapon[] { weapon[0], weapon[1] };
            return ret;
        }
    }

    public class Player
    {
        public int cash;

        public PlayerItems items = new PlayerItems(), last_items = new PlayerItems();

        public JE_boolean is_dragonwing;  // i.e., is player 2
        public int lives
        {
            get => items.weapon[is_dragonwing ? 0 : 1].power;
            set => items.weapon[is_dragonwing ? 0 : 1].power = (JE_word)value;
        }
        
        // calculatable
        public int shield_max;
        public int initial_armor;
        public int shot_hit_area_x, shot_hit_area_y;

        // state
        public JE_boolean is_alive;
        public int invulnerable_ticks;  // ticks until ship can be damaged
        public int exploding_ticks;     // ticks until ship done exploding
        public int shield;
        public int armor;
        public int weapon_mode;
        public int superbombs;
        public int purple_balls_needed;

        public int x, y;
        public int[] old_x = new int[20], old_y = new int[20];

        public JE_integer x_velocity, y_velocity;
        public JE_word x_friction_ticks, y_friction_ticks;  // ticks until friction is applied

        public JE_integer delta_x_shot_move, delta_y_shot_move;

        public int last_x_shot_move, last_y_shot_move;
        public JE_integer last_x_explosion_follow, last_y_explosion_follow;


        public class Sidekick
        {
            // calculatable
            public JE_integer ammo_max;
            public int ammo_refill_ticks_max;
            public JE_word style;  // affects movement and size

            // state
            public JE_integer x, y;
            public JE_integer ammo;
            public int ammo_refill_ticks;

            public JE_boolean animation_enabled;
            public JE_word animation_frame;

            public JE_word charge;
            public JE_word charge_ticks;
        }

        public Sidekick[] sidekick = new [] { new Sidekick(), new Sidekick() };
    }

    public static Player[] player = new[] { new Player(), new Player() };

    public static bool all_players_dead()
    {
        return (!player[0].is_alive && (!twoPlayerMode || !player[1].is_alive));
    }

    public static bool all_players_alive()
    {
        return (player[0].is_alive && (!twoPlayerMode || player[1].is_alive));
    }

    private static readonly int[] purple_balls_required = { 1, 1, 2, 4, 8, 12, 16, 20, 25, 30, 40, 50 };
    public static void calc_purple_balls_needed(Player this_player)
    {
        this_player.purple_balls_needed = purple_balls_required[this_player.lives];
    }

    public static bool power_up_weapon(Player this_player, int port)
    {
        bool can_power_up = this_player.items.weapon[port].id != 0 &&  // not None
                            this_player.items.weapon[port].power < 11; // not at max power
        if (can_power_up)
        {
            ++this_player.items.weapon[port].power;
            shotMultiPos[port] = 0; // TODO: should be part of Player structure

            calc_purple_balls_needed(this_player);
        }
        else  // cash consolation prize
        {
            this_player.cash += 1000;
        }

        return can_power_up;
    }

    public static void handle_got_purple_ball(Player this_player)
    {
        if (this_player.purple_balls_needed > 1)
            --this_player.purple_balls_needed;
        else
            power_up_weapon(this_player, this_player.is_dragonwing ? REAR_WEAPON : FRONT_WEAPON);
    }

}