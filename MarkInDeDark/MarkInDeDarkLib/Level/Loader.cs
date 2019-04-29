#region Using

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

#endregion

namespace Mark.Engine
{
    public static class Loader
    {
        #region Dichiarazioni

        private static Dictionary<string, Rectangle> textures_rectangles_ = new Dictionary<string, Rectangle>();
        public static Texture2D atlas_;
        public static SpriteFont font_;

        #region Liste texture

        public static List<Rectangle> p_easy_up_ = new List<Rectangle>();
        public static List<Rectangle> p_easy_down_ = new List<Rectangle>();
        public static List<Rectangle> p_easy_side_ = new List<Rectangle>();
        public static List<Rectangle> p_easy_up_idle_ = new List<Rectangle>();
        public static List<Rectangle> p_easy_down_idle_ = new List<Rectangle>();
        public static List<Rectangle> p_easy_side_idle_ = new List<Rectangle>();
        public static List<Rectangle> p_easy_dying_ = new List<Rectangle>();
        public static List<Rectangle> p_rotating_ = new List<Rectangle>();

        public static List<Rectangle> zombie_up_ = new List<Rectangle>();
        public static List<Rectangle> zombie_down_ = new List<Rectangle>();
        public static List<Rectangle> zombie_side_ = new List<Rectangle>();
        public static List<Rectangle> zombie_up_idle_ = new List<Rectangle>();
        public static List<Rectangle> zombie_down_idle_ = new List<Rectangle>();
        public static List<Rectangle> zombie_side_idle_ = new List<Rectangle>();
        public static List<Rectangle> zombie_up_attack_ = new List<Rectangle>();
        public static List<Rectangle> zombie_side_attack_ = new List<Rectangle>();
        public static List<Rectangle> zombie_down_attack_ = new List<Rectangle>();

        public static List<Rectangle> werewolf_up_ = new List<Rectangle>();
        public static List<Rectangle> werewolf_down_ = new List<Rectangle>();
        public static List<Rectangle> werewolf_side_ = new List<Rectangle>();
        public static List<Rectangle> werewolf_up_idle_ = new List<Rectangle>();
        public static List<Rectangle> werewolf_down_idle_ = new List<Rectangle>();
        public static List<Rectangle> werewolf_side_idle_ = new List<Rectangle>();
        public static List<Rectangle> werewolf_up_attack_ = new List<Rectangle>();
        public static List<Rectangle> werewolf_side_attack_ = new List<Rectangle>();
        public static List<Rectangle> werewolf_down_attack_ = new List<Rectangle>();

        public static List<Rectangle> goblin_up_ = new List<Rectangle>();
        public static List<Rectangle> goblin_down_ = new List<Rectangle>();
        public static List<Rectangle> goblin_side_ = new List<Rectangle>();
        public static List<Rectangle> goblin_up_idle_ = new List<Rectangle>();
        public static List<Rectangle> goblin_down_idle_ = new List<Rectangle>();
        public static List<Rectangle> goblin_side_idle_ = new List<Rectangle>();
        public static List<Rectangle> goblin_up_attack_ = new List<Rectangle>();
        public static List<Rectangle> goblin_side_attack_ = new List<Rectangle>();
        public static List<Rectangle> goblin_down_attack_ = new List<Rectangle>();

        public static List<Rectangle> skeletron_up_ = new List<Rectangle>();
        public static List<Rectangle> skeletron_down_ = new List<Rectangle>();
        public static List<Rectangle> skeletron_side_ = new List<Rectangle>();
        public static List<Rectangle> skeletron_up_idle_ = new List<Rectangle>();
        public static List<Rectangle> skeletron_down_idle_ = new List<Rectangle>();
        public static List<Rectangle> skeletron_side_idle_ = new List<Rectangle>();
        public static List<Rectangle> skeletron_up_attack_ = new List<Rectangle>();
        public static List<Rectangle> skeletron_side_attack_ = new List<Rectangle>();
        public static List<Rectangle> skeletron_down_attack_ = new List<Rectangle>();

        public static List<Rectangle> angle_ = new List<Rectangle>();
        public static List<Rectangle> cells_ = new List<Rectangle>();
        public static List<Rectangle> dirty_ = new List<Rectangle>();
        public static List<Rectangle> wall_ = new List<Rectangle>();
        public static List<Rectangle> macerie_ = new List<Rectangle>();
        public static List<Rectangle> secrets_ = new List<Rectangle>();


        public static List<Rectangle> candle_ = new List<Rectangle>();
        public static Rectangle key_;
        public static Rectangle blood_decal_;
        public static Rectangle blood_particle_;
        public static Rectangle smoke_;
        public static Rectangle win_;
        public static Rectangle win_closed_;
        public static Rectangle lightmask_;
        public static Rectangle light_pixel_;
        public static Rectangle speed_potion_;
        public static Rectangle scintilla_;
        public static Rectangle scintilla_lightmap_;
        
        public static Rectangle dpad_empty_;
        public static Rectangle dpad_down_;
        public static Rectangle dpad_up_;
        public static Rectangle dpad_right_;
        public static Rectangle dpad_left_;
        public static Rectangle dpad_buttonNormal_;
        public static Rectangle dpad_buttonPressed_;
        
        #endregion

        public static Dictionary<string, SoundEffect> sounds_ = new Dictionary<string, SoundEffect>();

        #endregion

        #region Loading

        public static void LoadRectangles()
        {
            if (textures_rectangles_.Count == 0)
            {
                #region fbonizzi Exporter Output

                textures_rectangles_.Add("ButtonNormal", new Rectangle(456, 156, 150, 150));
                textures_rectangles_.Add("ButtonPressed", new Rectangle(2, 304, 150, 150));
                textures_rectangles_.Add("DpadDown", new Rectangle(304, 156, 150, 152));
                textures_rectangles_.Add("DpadEmpty", new Rectangle(760, 2, 150, 152));
                textures_rectangles_.Add("DpadLeft", new Rectangle(608, 2, 150, 152));
                textures_rectangles_.Add("DpadRight", new Rectangle(456, 2, 150, 152));
                textures_rectangles_.Add("DpadUp", new Rectangle(304, 2, 150, 152));
                textures_rectangles_.Add("angle0", new Rectangle(236, 356, 50, 50));
                textures_rectangles_.Add("angle1", new Rectangle(854, 312, 50, 50));
                textures_rectangles_.Add("angle2", new Rectangle(854, 260, 50, 50));
                textures_rectangles_.Add("angle3", new Rectangle(538, 308, 50, 50));
                textures_rectangles_.Add("angle4", new Rectangle(854, 208, 50, 50));
                textures_rectangles_.Add("angle5", new Rectangle(854, 156, 50, 50));
                textures_rectangles_.Add("angle6", new Rectangle(236, 304, 50, 50));
                textures_rectangles_.Add("blood", new Rectangle(994, 46, 20, 20));
                textures_rectangles_.Add("blood_decal", new Rectangle(166, 1294, 80, 80));
                textures_rectangles_.Add("candle1", new Rectangle(330, 1292, 80, 80));
                textures_rectangles_.Add("candle2", new Rectangle(412, 1222, 80, 80));
                textures_rectangles_.Add("candle3", new Rectangle(904, 1068, 80, 80));
                textures_rectangles_.Add("candle4", new Rectangle(822, 1068, 80, 80));
                textures_rectangles_.Add("candle5", new Rectangle(658, 1144, 80, 80));
                textures_rectangles_.Add("candle6", new Rectangle(740, 1068, 80, 80));
                textures_rectangles_.Add("cell0", new Rectangle(576, 1140, 80, 80));
                textures_rectangles_.Add("cell1", new Rectangle(494, 1140, 80, 80));
                textures_rectangles_.Add("cell2", new Rectangle(248, 1212, 80, 80));
                textures_rectangles_.Add("cell3", new Rectangle(2, 1358, 80, 80));
                textures_rectangles_.Add("cell4", new Rectangle(84, 1288, 80, 80));
                textures_rectangles_.Add("cell5", new Rectangle(166, 1212, 80, 80));
                textures_rectangles_.Add("cell6", new Rectangle(330, 1210, 80, 80));
                textures_rectangles_.Add("cell7", new Rectangle(412, 1140, 80, 80));
                textures_rectangles_.Add("dirty0", new Rectangle(836, 364, 40, 40));
                textures_rectangles_.Add("dirty1", new Rectangle(84, 540, 40, 40));
                textures_rectangles_.Add("dirty2", new Rectangle(84, 498, 40, 40));
                textures_rectangles_.Add("dirty3", new Rectangle(236, 450, 40, 40));
                textures_rectangles_.Add("dirty4", new Rectangle(236, 408, 40, 40));
                textures_rectangles_.Add("dirty5", new Rectangle(538, 360, 40, 40));
                textures_rectangles_.Add("dirty6", new Rectangle(84, 456, 40, 40));
                textures_rectangles_.Add("easy_back_iddle", new Rectangle(904, 986, 80, 80));
                textures_rectangles_.Add("easy_back_walking_1", new Rectangle(822, 986, 80, 80));
                textures_rectangles_.Add("easy_back_walking_2", new Rectangle(658, 1062, 80, 80));
                textures_rectangles_.Add("easy_front_dying_1", new Rectangle(740, 986, 80, 80));
                textures_rectangles_.Add("easy_front_dying_2", new Rectangle(576, 1058, 80, 80));
                textures_rectangles_.Add("easy_front_dying_3", new Rectangle(494, 1058, 80, 80));
                textures_rectangles_.Add("easy_front_iddle", new Rectangle(248, 1130, 80, 80));
                textures_rectangles_.Add("easy_side_iddle", new Rectangle(2, 1276, 80, 80));
                textures_rectangles_.Add("easy_side_walking_1", new Rectangle(84, 1206, 80, 80));
                textures_rectangles_.Add("easy_side_walking_2", new Rectangle(166, 1130, 80, 80));
                textures_rectangles_.Add("easy_walking_1", new Rectangle(330, 1128, 80, 80));
                textures_rectangles_.Add("easy_walking_2", new Rectangle(412, 1058, 80, 80));
                textures_rectangles_.Add("goblin_back_attack_1", new Rectangle(904, 904, 80, 80));
                textures_rectangles_.Add("goblin_back_attack_2", new Rectangle(822, 904, 80, 80));
                textures_rectangles_.Add("goblin_back_iddle", new Rectangle(658, 980, 80, 80));
                textures_rectangles_.Add("goblin_back_walking_1", new Rectangle(740, 904, 80, 80));
                textures_rectangles_.Add("goblin_back_walking_2", new Rectangle(576, 976, 80, 80));
                textures_rectangles_.Add("goblin_front_attack_1", new Rectangle(494, 976, 80, 80));
                textures_rectangles_.Add("goblin_front_attack_2", new Rectangle(248, 1048, 80, 80));
                textures_rectangles_.Add("goblin_front_iddle", new Rectangle(2, 1194, 80, 80));
                textures_rectangles_.Add("goblin_front_walking_1", new Rectangle(84, 1124, 80, 80));
                textures_rectangles_.Add("goblin_front_walking_2", new Rectangle(166, 1048, 80, 80));
                textures_rectangles_.Add("goblin_side_attack_1", new Rectangle(330, 1046, 80, 80));
                textures_rectangles_.Add("goblin_side_attack_2", new Rectangle(412, 976, 80, 80));
                textures_rectangles_.Add("goblin_side_iddle", new Rectangle(904, 822, 80, 80));
                textures_rectangles_.Add("goblin_side_walking_1", new Rectangle(822, 822, 80, 80));
                textures_rectangles_.Add("goblin_side_walking_2", new Rectangle(658, 898, 80, 80));
                textures_rectangles_.Add("key", new Rectangle(740, 822, 80, 80));
                textures_rectangles_.Add("light_1", new Rectangle(2, 2, 300, 300));
                textures_rectangles_.Add("light_pixel", new Rectangle(576, 894, 80, 80));
                textures_rectangles_.Add("macerie0", new Rectangle(494, 894, 80, 80));
                textures_rectangles_.Add("macerie1", new Rectangle(248, 966, 80, 80));
                textures_rectangles_.Add("macerie2", new Rectangle(2, 1112, 80, 80));
                textures_rectangles_.Add("macerie3", new Rectangle(84, 1042, 80, 80));
                textures_rectangles_.Add("macerie4", new Rectangle(166, 966, 80, 80));
                textures_rectangles_.Add("macerie5", new Rectangle(330, 964, 80, 80));
                textures_rectangles_.Add("macerie6", new Rectangle(412, 894, 80, 80));
                textures_rectangles_.Add("macerie7", new Rectangle(904, 740, 80, 80));
                textures_rectangles_.Add("scintilla", new Rectangle(994, 24, 20, 20));
                textures_rectangles_.Add("scintilla_lightmap", new Rectangle(994, 24, 20, 20));
                textures_rectangles_.Add("segreto (1)", new Rectangle(822, 740, 80, 80));
                textures_rectangles_.Add("segreto (10)", new Rectangle(658, 816, 80, 80));
                textures_rectangles_.Add("segreto (11)", new Rectangle(740, 740, 80, 80));
                textures_rectangles_.Add("segreto (12)", new Rectangle(576, 812, 80, 80));
                textures_rectangles_.Add("segreto (13)", new Rectangle(494, 812, 80, 80));
                textures_rectangles_.Add("segreto (14)", new Rectangle(248, 884, 80, 80));
                textures_rectangles_.Add("segreto (15)", new Rectangle(2, 1030, 80, 80));
                textures_rectangles_.Add("segreto (16)", new Rectangle(84, 960, 80, 80));
                textures_rectangles_.Add("segreto (17)", new Rectangle(166, 884, 80, 80));
                textures_rectangles_.Add("segreto (18)", new Rectangle(330, 882, 80, 80));
                textures_rectangles_.Add("segreto (19)", new Rectangle(412, 812, 80, 80));
                textures_rectangles_.Add("segreto (2)", new Rectangle(658, 734, 80, 80));
                textures_rectangles_.Add("segreto (20)", new Rectangle(576, 730, 80, 80));
                textures_rectangles_.Add("segreto (21)", new Rectangle(494, 730, 80, 80));
                textures_rectangles_.Add("segreto (22)", new Rectangle(248, 802, 80, 80));
                textures_rectangles_.Add("segreto (23)", new Rectangle(2, 948, 80, 80));
                textures_rectangles_.Add("segreto (24)", new Rectangle(84, 878, 80, 80));
                textures_rectangles_.Add("segreto (25)", new Rectangle(166, 802, 80, 80));
                textures_rectangles_.Add("segreto (26)", new Rectangle(330, 800, 80, 80));
                textures_rectangles_.Add("segreto (27)", new Rectangle(412, 730, 80, 80));
                textures_rectangles_.Add("segreto (28)", new Rectangle(864, 658, 80, 80));
                textures_rectangles_.Add("segreto (29)", new Rectangle(782, 658, 80, 80));
                textures_rectangles_.Add("segreto (3)", new Rectangle(700, 652, 80, 80));
                textures_rectangles_.Add("segreto (30)", new Rectangle(618, 648, 80, 80));
                textures_rectangles_.Add("segreto (31)", new Rectangle(536, 648, 80, 80));
                textures_rectangles_.Add("segreto (32)", new Rectangle(248, 720, 80, 80));
                textures_rectangles_.Add("segreto (33)", new Rectangle(2, 866, 80, 80));
                textures_rectangles_.Add("segreto (34)", new Rectangle(84, 796, 80, 80));
                textures_rectangles_.Add("segreto (35)", new Rectangle(166, 720, 80, 80));
                textures_rectangles_.Add("segreto (36)", new Rectangle(330, 718, 80, 80));
                textures_rectangles_.Add("segreto (37)", new Rectangle(454, 648, 80, 80));
                textures_rectangles_.Add("segreto (38)", new Rectangle(248, 638, 80, 80));
                textures_rectangles_.Add("segreto (39)", new Rectangle(2, 784, 80, 80));
                textures_rectangles_.Add("segreto (4)", new Rectangle(84, 714, 80, 80));
                textures_rectangles_.Add("segreto (40)", new Rectangle(166, 638, 80, 80));
                textures_rectangles_.Add("segreto (5)", new Rectangle(934, 576, 80, 80));
                textures_rectangles_.Add("segreto (6)", new Rectangle(852, 576, 80, 80));
                textures_rectangles_.Add("segreto (7)", new Rectangle(770, 570, 80, 80));
                textures_rectangles_.Add("segreto (8)", new Rectangle(688, 566, 80, 80));
                textures_rectangles_.Add("segreto (9)", new Rectangle(606, 566, 80, 80));
                textures_rectangles_.Add("skeleton_back_attack_1", new Rectangle(372, 636, 80, 80));
                textures_rectangles_.Add("skeleton_back_attack_2", new Rectangle(524, 566, 80, 80));
                textures_rectangles_.Add("skeleton_back_iddle", new Rectangle(290, 556, 80, 80));
                textures_rectangles_.Add("skeleton_back_walking_1", new Rectangle(2, 702, 80, 80));
                textures_rectangles_.Add("skeleton_back_walking_2", new Rectangle(84, 632, 80, 80));
                textures_rectangles_.Add("skeleton_front_attack_1", new Rectangle(208, 556, 80, 80));
                textures_rectangles_.Add("skeleton_front_attack_2", new Rectangle(862, 494, 80, 80));
                textures_rectangles_.Add("skeleton_front_iddle", new Rectangle(780, 488, 80, 80));
                textures_rectangles_.Add("skeleton_front_walking_1", new Rectangle(698, 484, 80, 80));
                textures_rectangles_.Add("skeleton_front_walking_2", new Rectangle(616, 484, 80, 80));
                textures_rectangles_.Add("skeleton_side_attack_1", new Rectangle(442, 554, 80, 80));
                textures_rectangles_.Add("skeleton_side_attack_2", new Rectangle(534, 484, 80, 80));
                textures_rectangles_.Add("skeleton_side_iddle", new Rectangle(862, 412, 80, 80));
                textures_rectangles_.Add("skeleton_side_walking_1", new Rectangle(780, 406, 80, 80));
                textures_rectangles_.Add("skeleton_side_walking_2", new Rectangle(698, 402, 80, 80));
                textures_rectangles_.Add("smoke", new Rectangle(994, 2, 20, 20));
                textures_rectangles_.Add("wall0", new Rectangle(904, 1150, 80, 38));
                textures_rectangles_.Add("wall1", new Rectangle(822, 1150, 80, 38));
                textures_rectangles_.Add("wall2", new Rectangle(740, 1150, 80, 38));
                textures_rectangles_.Add("wall3", new Rectangle(576, 1222, 80, 38));
                textures_rectangles_.Add("wall4", new Rectangle(494, 1222, 80, 38));
                textures_rectangles_.Add("wall5", new Rectangle(248, 1334, 80, 38));
                textures_rectangles_.Add("wall6", new Rectangle(248, 1294, 80, 38));
                textures_rectangles_.Add("wall7", new Rectangle(84, 1370, 80, 38));
                textures_rectangles_.Add("werewolf_back_attack_1", new Rectangle(616, 402, 80, 80));
                textures_rectangles_.Add("werewolf_back_attack_2", new Rectangle(2, 620, 80, 80));
                textures_rectangles_.Add("werewolf_back_iddle", new Rectangle(126, 550, 80, 80));
                textures_rectangles_.Add("werewolf_back_walking_1", new Rectangle(360, 474, 80, 80));
                textures_rectangles_.Add("werewolf_back_walking_2", new Rectangle(278, 474, 80, 80));
                textures_rectangles_.Add("werewolf_front_attack_1", new Rectangle(452, 472, 80, 80));
                textures_rectangles_.Add("werewolf_front_attack_2", new Rectangle(534, 402, 80, 80));
                textures_rectangles_.Add("werewolf_front_iddle", new Rectangle(754, 320, 80, 80));
                textures_rectangles_.Add("werewolf_front_walking_1", new Rectangle(672, 320, 80, 80));
                textures_rectangles_.Add("werewolf_front_walking_2", new Rectangle(2, 538, 80, 80));
                textures_rectangles_.Add("werewolf_side_attack_1", new Rectangle(126, 468, 80, 80));
                textures_rectangles_.Add("werewolf_side_attack_2", new Rectangle(370, 392, 80, 80));
                textures_rectangles_.Add("werewolf_side_iddle", new Rectangle(288, 392, 80, 80));
                textures_rectangles_.Add("werewolf_side_walking_1", new Rectangle(452, 390, 80, 80));
                textures_rectangles_.Add("werewolf_side_walking_2", new Rectangle(590, 320, 80, 80));
                textures_rectangles_.Add("win", new Rectangle(2, 456, 80, 80));
                textures_rectangles_.Add("win_closed", new Rectangle(370, 310, 80, 80));
                textures_rectangles_.Add("zombie_back_attack_1", new Rectangle(154, 386, 80, 80));
                textures_rectangles_.Add("zombie_back_attack_2", new Rectangle(288, 310, 80, 80));
                textures_rectangles_.Add("zombie_back_iddle", new Rectangle(906, 330, 80, 80));
                textures_rectangles_.Add("zombie_back_walking_1", new Rectangle(772, 238, 80, 80));
                textures_rectangles_.Add("zombie_back_walking_2", new Rectangle(690, 238, 80, 80));
                textures_rectangles_.Add("zombie_front_attack_1", new Rectangle(456, 308, 80, 80));
                textures_rectangles_.Add("zombie_front_attack_2", new Rectangle(608, 238, 80, 80));
                textures_rectangles_.Add("zombie_front_iddle", new Rectangle(906, 248, 80, 80));
                textures_rectangles_.Add("zombie_front_walking_1", new Rectangle(906, 166, 80, 80));
                textures_rectangles_.Add("zombie_front_walking_2", new Rectangle(772, 156, 80, 80));
                textures_rectangles_.Add("zombie_side_attack_1", new Rectangle(690, 156, 80, 80));
                textures_rectangles_.Add("zombie_side_attack_2", new Rectangle(608, 156, 80, 80));
                textures_rectangles_.Add("zombie_side_iddle", new Rectangle(154, 304, 80, 80));
                textures_rectangles_.Add("zombie_side_walking_1", new Rectangle(912, 84, 80, 80));
                textures_rectangles_.Add("zombie_side_walking_2", new Rectangle(912, 2, 80, 80));


                #endregion

                #region Frames

                #region CHARACTERS

                /* PLAYER */
                p_easy_up_.Add(textures_rectangles_["easy_back_walking_1"]);
                p_easy_up_.Add(textures_rectangles_["easy_back_iddle"]);
                p_easy_up_.Add(textures_rectangles_["easy_back_walking_2"]);
                p_easy_down_.Add(textures_rectangles_["easy_walking_1"]);
                p_easy_down_.Add(textures_rectangles_["easy_front_iddle"]);
                p_easy_down_.Add(textures_rectangles_["easy_walking_2"]);
                p_easy_side_.Add(textures_rectangles_["easy_side_walking_1"]);
                p_easy_side_.Add(textures_rectangles_["easy_side_iddle"]);
                p_easy_side_.Add(textures_rectangles_["easy_side_walking_2"]);
                p_easy_side_.Add(textures_rectangles_["easy_side_iddle"]);
                p_easy_up_idle_.Add(textures_rectangles_["easy_back_iddle"]);
                p_easy_down_idle_.Add(textures_rectangles_["easy_front_iddle"]);
                p_easy_side_idle_.Add(textures_rectangles_["easy_side_iddle"]);
                p_easy_dying_.Add(textures_rectangles_["easy_front_dying_1"]);
                p_easy_dying_.Add(textures_rectangles_["easy_front_dying_2"]);
                p_easy_dying_.Add(textures_rectangles_["easy_front_dying_3"]);
                p_rotating_.Add(textures_rectangles_["easy_back_iddle"]);
                p_rotating_.Add(textures_rectangles_["easy_front_iddle"]);
                p_rotating_.Add(textures_rectangles_["easy_side_iddle"]);

                /* GOBLIN */
                goblin_up_.Add(textures_rectangles_["goblin_back_walking_1"]);
                goblin_up_.Add(textures_rectangles_["goblin_back_iddle"]);
                goblin_up_.Add(textures_rectangles_["goblin_back_walking_2"]);
                goblin_down_.Add(textures_rectangles_["goblin_front_walking_1"]);
                goblin_down_.Add(textures_rectangles_["goblin_front_iddle"]);
                goblin_down_.Add(textures_rectangles_["goblin_front_walking_2"]);
                goblin_side_.Add(textures_rectangles_["goblin_side_walking_1"]);
                goblin_side_.Add(textures_rectangles_["goblin_side_iddle"]);
                goblin_side_.Add(textures_rectangles_["goblin_side_walking_2"]);
                goblin_up_idle_.Add(textures_rectangles_["goblin_back_iddle"]);
                goblin_down_idle_.Add(textures_rectangles_["goblin_front_iddle"]);
                goblin_side_idle_.Add(textures_rectangles_["goblin_side_iddle"]);
                goblin_up_attack_.Add(textures_rectangles_["goblin_back_attack_1"]);
                goblin_up_attack_.Add(textures_rectangles_["goblin_back_attack_2"]);
                goblin_side_attack_.Add(textures_rectangles_["goblin_side_attack_1"]);
                goblin_side_attack_.Add(textures_rectangles_["goblin_side_attack_2"]);
                goblin_down_attack_.Add(textures_rectangles_["goblin_front_attack_1"]);
                goblin_down_attack_.Add(textures_rectangles_["goblin_front_attack_2"]);

                /* WEREWOLF */
                werewolf_up_.Add(textures_rectangles_["werewolf_back_walking_1"]);
                werewolf_up_.Add(textures_rectangles_["werewolf_back_iddle"]);
                werewolf_up_.Add(textures_rectangles_["werewolf_back_walking_2"]);
                werewolf_down_.Add(textures_rectangles_["werewolf_front_walking_1"]);
                werewolf_down_.Add(textures_rectangles_["werewolf_front_iddle"]);
                werewolf_down_.Add(textures_rectangles_["werewolf_front_walking_2"]);
                werewolf_side_.Add(textures_rectangles_["werewolf_side_walking_1"]);
                werewolf_side_.Add(textures_rectangles_["werewolf_side_iddle"]);
                werewolf_side_.Add(textures_rectangles_["werewolf_side_walking_2"]);
                werewolf_up_idle_.Add(textures_rectangles_["werewolf_back_iddle"]);
                werewolf_down_idle_.Add(textures_rectangles_["werewolf_front_iddle"]);
                werewolf_side_idle_.Add(textures_rectangles_["werewolf_side_iddle"]);
                werewolf_up_attack_.Add(textures_rectangles_["werewolf_back_attack_1"]);
                werewolf_up_attack_.Add(textures_rectangles_["werewolf_back_attack_2"]);
                werewolf_side_attack_.Add(textures_rectangles_["werewolf_side_attack_1"]);
                werewolf_side_attack_.Add(textures_rectangles_["werewolf_side_attack_2"]);
                werewolf_down_attack_.Add(textures_rectangles_["werewolf_front_attack_1"]);
                werewolf_down_attack_.Add(textures_rectangles_["werewolf_front_attack_2"]);


                /* SKELETRON */
                skeletron_up_.Add(textures_rectangles_["skeleton_back_walking_1"]);
                skeletron_up_.Add(textures_rectangles_["skeleton_back_iddle"]);
                skeletron_up_.Add(textures_rectangles_["skeleton_back_walking_2"]);
                skeletron_down_.Add(textures_rectangles_["skeleton_front_walking_1"]);
                skeletron_down_.Add(textures_rectangles_["skeleton_front_iddle"]);
                skeletron_down_.Add(textures_rectangles_["skeleton_front_walking_2"]);
                skeletron_side_.Add(textures_rectangles_["skeleton_side_walking_1"]);
                skeletron_side_.Add(textures_rectangles_["skeleton_side_iddle"]);
                skeletron_side_.Add(textures_rectangles_["skeleton_side_walking_2"]);
                skeletron_up_idle_.Add(textures_rectangles_["skeleton_back_iddle"]);
                skeletron_down_idle_.Add(textures_rectangles_["skeleton_front_iddle"]);
                skeletron_side_idle_.Add(textures_rectangles_["skeleton_side_iddle"]);
                skeletron_up_attack_.Add(textures_rectangles_["skeleton_back_attack_1"]);
                skeletron_up_attack_.Add(textures_rectangles_["skeleton_back_attack_2"]);
                skeletron_side_attack_.Add(textures_rectangles_["skeleton_side_attack_1"]);
                skeletron_side_attack_.Add(textures_rectangles_["skeleton_side_attack_2"]);
                skeletron_down_attack_.Add(textures_rectangles_["skeleton_front_attack_1"]);
                skeletron_down_attack_.Add(textures_rectangles_["skeleton_front_attack_2"]);

                /* ZOMBIE */
                zombie_up_.Add(textures_rectangles_["zombie_back_walking_1"]);
                zombie_up_.Add(textures_rectangles_["zombie_back_iddle"]);
                zombie_up_.Add(textures_rectangles_["zombie_back_walking_2"]);
                zombie_down_.Add(textures_rectangles_["zombie_front_walking_1"]);
                zombie_down_.Add(textures_rectangles_["zombie_front_iddle"]);
                zombie_down_.Add(textures_rectangles_["zombie_front_walking_2"]);
                zombie_side_.Add(textures_rectangles_["zombie_side_walking_1"]);
                zombie_side_.Add(textures_rectangles_["zombie_side_iddle"]);
                zombie_side_.Add(textures_rectangles_["zombie_side_walking_2"]);
                zombie_up_idle_.Add(textures_rectangles_["zombie_back_iddle"]);
                zombie_down_idle_.Add(textures_rectangles_["zombie_front_iddle"]);
                zombie_side_idle_.Add(textures_rectangles_["zombie_side_iddle"]);
                zombie_up_attack_.Add(textures_rectangles_["zombie_back_attack_1"]);
                zombie_up_attack_.Add(textures_rectangles_["zombie_back_attack_2"]);
                zombie_side_attack_.Add(textures_rectangles_["zombie_side_attack_1"]);
                zombie_side_attack_.Add(textures_rectangles_["zombie_side_attack_2"]);
                zombie_down_attack_.Add(textures_rectangles_["zombie_front_attack_1"]);
                zombie_down_attack_.Add(textures_rectangles_["zombie_front_attack_2"]);

                #endregion

                #region OTHER

                /* CANDLE */
                candle_.Add(textures_rectangles_["candle1"]);
                candle_.Add(textures_rectangles_["candle2"]);
                candle_.Add(textures_rectangles_["candle3"]);
                candle_.Add(textures_rectangles_["candle4"]);
                candle_.Add(textures_rectangles_["candle5"]);
                candle_.Add(textures_rectangles_["candle6"]);

                /* OTHER */
                key_ = textures_rectangles_["key"];
                blood_decal_ = textures_rectangles_["blood_decal"];
                blood_particle_ = textures_rectangles_["blood"];
                smoke_ = textures_rectangles_["smoke"];
                win_ = textures_rectangles_["win"];
                win_closed_ = textures_rectangles_["win_closed"];
                lightmask_ = textures_rectangles_["light_1"];
                light_pixel_ = textures_rectangles_["light_pixel"];
                scintilla_ = textures_rectangles_["scintilla"];
                scintilla_lightmap_ = textures_rectangles_["scintilla_lightmap"];

                /* INPUT */
                dpad_buttonNormal_ = textures_rectangles_["ButtonNormal"];
                dpad_buttonPressed_ = textures_rectangles_["ButtonPressed"];

                dpad_down_ = textures_rectangles_["DpadDown"];
                dpad_up_ = textures_rectangles_["DpadUp"];
                dpad_left_= textures_rectangles_["DpadLeft"];
                dpad_right_ = textures_rectangles_["DpadRight"];
                dpad_empty_ = textures_rectangles_["DpadEmpty"];

                #endregion

                #region CELLS

                // CELLE
                bool last = false;
                int j = 0;
                while (!last)
                {
                    if (textures_rectangles_.ContainsKey("cell" + j))
                        cells_.Add(textures_rectangles_["cell" + j]);
                    else
                        last = true;

                    ++j;
                }

                // DIRTY
                last = false;
                j = 0;
                while (!last)
                {
                    if (textures_rectangles_.ContainsKey("dirty" + j))
                        dirty_.Add(textures_rectangles_["dirty" + j]);
                    else
                        last = true;

                    ++j;
                }

                // MURI
                last = false;
                j = 0;
                while (!last)
                {
                    if (textures_rectangles_.ContainsKey("wall" + j))
                        wall_.Add(textures_rectangles_["wall" + j]);
                    else
                        last = true;

                    ++j;
                }

                // ANGLE
                last = false;
                j = 0;
                while (!last)
                {
                    if (textures_rectangles_.ContainsKey("angle" + j))
                        angle_.Add(textures_rectangles_["angle" + j]);
                    else
                        last = true;

                    ++j;
                }


                // MACERIE
                last = false;
                j = 0;
                while (!last)
                {
                    if (textures_rectangles_.ContainsKey("macerie" + j))
                        macerie_.Add(textures_rectangles_["macerie" + j]);
                    else
                        last = true;

                    ++j;
                }

                // SECRETS
                last = false;
                j = 1;
                while (!last)
                {
                    if (textures_rectangles_.ContainsKey("segreto (" + j + ")"))
                        secrets_.Add(textures_rectangles_["segreto (" + j + ")"]);
                    else
                        last = true;

                    ++j;
                }

                #endregion

                #endregion
            }
        }

        public static void LoadFiles(ContentManager Content)
        {
            if (font_ == null)
                font_ = Content.Load<SpriteFont>("font");
            if (atlas_ == null)
                atlas_ = Content.Load<Texture2D>("atlas");

            if (sounds_.Count == 0)
            {
                sounds_.Add("attack1", Content.Load<SoundEffect>("Soundtrack/attack1"));
                sounds_.Add("attack2", Content.Load<SoundEffect>("Soundtrack/attack2"));
                sounds_.Add("attack3", Content.Load<SoundEffect>("Soundtrack/attack3"));
                sounds_.Add("attack4", Content.Load<SoundEffect>("Soundtrack/attack4"));
                sounds_.Add("accendi_fiamma", Content.Load<SoundEffect>("Soundtrack/accendi_fiamma"));
                sounds_.Add("fuoco", Content.Load<SoundEffect>("Soundtrack/fuoco"));
                sounds_.Add("player_walk", Content.Load<SoundEffect>("Soundtrack/player_walk"));
                sounds_.Add("key", Content.Load<SoundEffect>("Soundtrack/key"));
                sounds_.Add("blood", Content.Load<SoundEffect>("Soundtrack/blood"));
                sounds_.Add("win", Content.Load<SoundEffect>("Soundtrack/win"));
                sounds_.Add("candela", Content.Load<SoundEffect>("Soundtrack/candela"));
                sounds_.Add("zombie_voice", Content.Load<SoundEffect>("Soundtrack/zombie_voice"));
                sounds_.Add("goblin_voice", Content.Load<SoundEffect>("Soundtrack/goblin_voice"));
                sounds_.Add("skeletron_voice", Content.Load<SoundEffect>("Soundtrack/skeletron_voice"));
                sounds_.Add("wolf_voice", Content.Load<SoundEffect>("Soundtrack/wolf_voice"));
            }
        }

        #endregion
    }
}
