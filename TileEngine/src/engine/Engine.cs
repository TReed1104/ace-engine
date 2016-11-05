﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using static TileEngine.Level;

namespace TileEngine
{
    public static class Engine
    {
        #region // Engine Vars
        public static string EngineName { get; private set; }
        public static string EngineVersion { get; private set; }
        #endregion
        #region // XNA Vars
        public static GraphicsDeviceManager GraphicsDevideManager { get; set; }
        public static SpriteBatch SpriteBatch { get; set; }
        #endregion
        #region // Directory Vars
        public const string ConfigFileName_Engine = "engine.ini";
        public const string ConfigFileName_Tileset = "tileset.ini";

        public const string ConfigDirectory_Engine = "config/";
        public const string ConfigDirectory_Levels = "content/levels/";
        public const string ConfigDirectory_SaveData = "content/data/";
        public const string ConfigDirectory_Textures = "content/textures/";

        public static string ConfigFullPath_EngineConfig { get { return Engine.ConfigDirectory_Engine + Engine.ConfigFileName_Engine; } }
        public static string ConfigFullPath_Tileset { get { return Engine.ConfigDirectory_Engine + Engine.ConfigFileName_Tileset; } }
        #endregion
        #region // Window Vars
        public static string Window_Title { get; private set; }
        public static int FrameRate_Max { get; private set; }
        public static Matrix Window_TransformationMatrix { get; set; }
        public static float Window_Scaler { get; set; }
        public static Vector2 Window_HUD_Size_Tiles { get; set; }
        public static Vector2 Window_HUD_Size_Pixels { get { return Window_HUD_Size_Tiles * Tile.Dimensions; } }
        public static Vector2 Window_TileGridSize { get; private set; }
        public static Vector2 Window_PixelGridSize { get { return (Engine.Window_TileGridSize * Tile.Dimensions); } }
        public static Vector2 Window_DimensionsPixels_Base { get { return (Engine.Window_TileGridSize * Tile.Dimensions); } }
        public static Vector2 Window_DimensionsPixels_Scaled { get { return Engine.Window_DimensionsPixels_Base * Engine.Window_Scaler; } }

        public static Vector2 Camera_RenderGridSize_Tiles { get { return Engine.Window_TileGridSize - Engine.Window_HUD_Size_Tiles; } }
        #endregion
        #region // Register Vars
        public static List<Tile> Register_Tiles { get; set; }
        public static List<Level> Register_Levels { get; set; }
        public static List<Player> Register_Players { get; set; }
        public static List<Entity> Register_Npc { get; set; }
        public static List<Texture2D> Register_Textures { get; set; }
        #endregion
        #region // Pointer Vars
        public static int PointerCurrent_Player { get; set; }
        public static int PointerCurrent_Level { get; set; }
        #endregion
        #region // Counter Vars
        public static int Counter_Tiles { get { return Register_Tiles.Count; } }
        public static int Counter_Levels { get { return Register_Levels.Count; } }
        public static int Counter_Players { get { return Register_Players.Count; } }
        public static int Counter_Npcs { get { return Register_Npc.Count; } }
        #endregion
        #region // LayerDepth Vars
        public const float LayerDepth_Debugger_Background = 0.10f;
        public const float LayerDepth_Debugger_Terrain = 0.09f;
        public const float LayerDepth_Debugger_Interactive = 0.08f;
        public const float LayerDepth_Debugger_NPC = 0.07f;
        public const float LayerDepth_Debugger_Player = 0.06f;
        public const float LayerDepth_Debugger_Foreground = 0.05f;

        public const float LayerDepth_Background = 0.50f;
        public const float LayerDepth_Terrain = 0.09f;
        public const float LayerDepth_Interactive = 0.08f;
        public const float LayerDepth_NPC = 0.07f;
        public const float LayerDepth_Player = 0.06f;
        public const float LayerDepth_Foreground = 0.05f;
        #endregion
        #region // Camera Vars
        public static Camera MainCamera { get; set; }
        #endregion
        #region // Debugger Vars
        public static bool VisualDebugger { get; set; }
        #endregion

        // Constructors
        static Engine()
        {
            Engine.EngineName = "NULL";
            Engine.EngineVersion = "NULL";

            Engine.Window_Scaler = 1.0f;
            Engine.Window_Title = "NULL";
            Engine.FrameRate_Max = 30;
            Engine.Window_HUD_Size_Tiles = new Vector2(0, 50);
            Engine.Window_TileGridSize = new Vector2(10, 10);

            Engine.Register_Tiles = new List<Tile>();
            Engine.Register_Levels = new List<Level>();
            Engine.Register_Players = new List<Player>();
            Engine.Register_Npc = new List<Entity>();
            Engine.Register_Textures = new List<Texture2D>();

            Engine.PointerCurrent_Player = 0;
            Engine.PointerCurrent_Level = 0;

            Engine.MainCamera = new Camera("Main Camera", Vector2.Zero);

            Engine.VisualDebugger = false;

            Engine.Load();
            Engine.ClearLevelCache();   // Clear the cache for re-generation for testing.

            // Test generation
            Level temp = new Level("");
            temp.Generate(Randomiser.RandomString(6), Engine.Register_Levels.Count);
            Engine.Register_Levels.Add(temp);


        }
        // XNA Methods
        public static void Update(GameTime gameTime)
        {
            try
            {
                if (Engine.Register_Levels.Count > 0 && Engine.GetCurrentLevel() != null)
                {
                    Engine.GetCurrentLevel().Update(gameTime);
                }
                if (Engine.Register_Players.Count > 0)
                {
                    Engine.GetCurrentPlayer().Update(gameTime);
                }
                Engine.MainCamera.Update(gameTime, Engine.GetCurrentPlayer());
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
            }
        }
        public static void Draw(GameTime gameTime)
        {
            try
            {
                Engine.SpriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone, null, Engine.Window_TransformationMatrix);
                if (Engine.Register_Levels.Count > 0 && Engine.GetCurrentLevel() != null)
                {
                    Engine.GetCurrentLevel().Draw();
                }
                if (Engine.Register_Players.Count > 0)
                {
                    GetCurrentPlayer().Draw();
                }
                Engine.SpriteBatch.End();
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
            }
        }
        // Get methods for ease of access.
        public static Player GetCurrentPlayer()
        {
            try
            {
                return Engine.Register_Players[Engine.PointerCurrent_Player];
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
                return null;
            }
        }
        public static Level GetCurrentLevel()
        {
            try
            {
                return Engine.Register_Levels[Engine.PointerCurrent_Level];
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
                return null;
            }
        }
        // Engine loading methods
        public static void Load()
        {
            try
            {
                Engine.LoadEngineConfig();
                Engine.LoadTileset();
                Engine.LoadLevels();
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
            }
        }
        public static void LoadEngineConfig()
        {
            try
            {
                // Check for the config file.
                if (File.Exists(Engine.ConfigFullPath_EngineConfig))
                {
                    // Read the config file.
                    XmlReader xmlReader = XmlReader.Create(Engine.ConfigFullPath_EngineConfig);
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == XmlNodeType.Element)
                        {
                            if (xmlReader.Name == "engine_settings")
                            {
                                // Load the Engine settings
                                Engine.EngineName = xmlReader.GetAttribute("name");
                                Engine.EngineVersion = xmlReader.GetAttribute("version");
                            }
                            if (xmlReader.Name == "window_settings")
                            {
                                // Load the Window settings
                                Engine.Window_Title = xmlReader.GetAttribute("title");
                                Engine.Window_TileGridSize = new Vector2(int.Parse(xmlReader.GetAttribute("width")), int.Parse(xmlReader.GetAttribute("height")));
                                Engine.FrameRate_Max = int.Parse(xmlReader.GetAttribute("max_frame_rate"));
                                Engine.Window_Scaler = float.Parse(xmlReader.GetAttribute("scaler"));
                                Engine.Window_HUD_Size_Tiles = new Vector2(0, int.Parse(xmlReader.GetAttribute("hud_size")));
                            }
                            if (xmlReader.Name == "tile_settings")
                            {
                                // Load the Tileset settings
                                Tile.Dimensions = new Vector2(int.Parse(xmlReader.GetAttribute("width")), int.Parse(xmlReader.GetAttribute("height")));
                            }
                        }
                    }
                    xmlReader.Close();
                }
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
            }
        }
        public static void LoadTileset()
        {
            try
            {
                // Check for the config file.
                if (File.Exists(Engine.ConfigFullPath_Tileset))
                {
                    // Read the config file.
                    XmlReader xmlReader = XmlReader.Create(Engine.ConfigFullPath_Tileset);
                    while (xmlReader.Read())
                    {
                        if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "tileset")
                        {
                            Tile.TileSetTags = xmlReader.GetAttribute("tag");
                        }
                        if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "tile")
                        {
                            // Load the tile
                            string tag = xmlReader.GetAttribute("tag");
                            int src_frame_x = int.Parse(xmlReader.GetAttribute("src_frame_x"));
                            int src_frame_y = int.Parse(xmlReader.GetAttribute("src_frame_y"));
                            Color colour = Tile.Register_ConvertColour(xmlReader.GetAttribute("colour"));
                            int id = int.Parse(xmlReader.GetAttribute("id"));
                            Tile.TileType tileType = Tile.Register_ConvertTileType(xmlReader.GetAttribute("type"));

                            // Add the tile to the register ready for use.
                            Engine.Register_Tiles.Add(new Tile(tag, new Vector2(src_frame_x, src_frame_y), colour, Engine.LayerDepth_Terrain, id, tileType));
                        }
                    }
                    xmlReader.Close();
                }
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
            }
        }
        public static void LoadLevels()
        {
            try
            {

                if (!Directory.Exists(Engine.ConfigDirectory_Levels))
                {
                    Directory.CreateDirectory(Engine.ConfigDirectory_Levels);
                }
                string[] levelList = Directory.GetFiles(Engine.ConfigDirectory_Levels);

                for (int i = 0; i < levelList.Length; i++)
                {
                    Register_Levels.Add(new Level(levelList[i]));
                }
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
            }
        }
        public static void AssignTextures()
        {
            int indexOfTileSet = Engine.Register_Textures.FindIndex(r => r.Name == "textures/" + Tile.TileSetTags);
            Tile.TileSet = Engine.Register_Textures[indexOfTileSet];
        }
        // Engine Clear Cache methods
        public static void ClearLevelCache()
        {
            try
            {

                if (Directory.Exists(Engine.ConfigDirectory_Levels))
                {
                    Directory.Delete(Engine.ConfigDirectory_Levels, true);
                    Engine.Register_Levels.Clear();
                    Directory.CreateDirectory(Engine.ConfigDirectory_Levels);
                }
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
            }
        }
        // Engine Setup
        public static void InitialiseGameWindow()
        {
            try
            {
                Engine.Window_TransformationMatrix = Matrix.Identity;
                Engine.Window_TransformationMatrix *= Matrix.CreateScale(Engine.Window_Scaler);
                Engine.GraphicsDevideManager.PreferredBackBufferWidth = (int)Engine.Window_DimensionsPixels_Scaled.X;
                Engine.GraphicsDevideManager.PreferredBackBufferHeight = (int)Engine.Window_DimensionsPixels_Scaled.Y;
                Engine.GraphicsDevideManager.ApplyChanges();
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
            }
        }
        public static void CreateBlankSave(string playerName)
        {
            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(Engine.ConfigDirectory_SaveData + playerName + ".dat"))
                {
                    #region // Write a default save file
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteWhitespace("\r\n");
                    xmlWriter.WriteStartElement("save_data");
                    xmlWriter.WriteWhitespace("\r\n\t");

                    xmlWriter.WriteStartElement("tag");
                    xmlWriter.WriteAttributeString("value", "Player");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteWhitespace("\r\n\t");

                    xmlWriter.WriteStartElement("src_frame_pos_x");
                    xmlWriter.WriteAttributeString("value", "0");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteWhitespace("\r\n\t");

                    xmlWriter.WriteStartElement("src_frame_pos_y");
                    xmlWriter.WriteAttributeString("value", "0");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteWhitespace("\r\n\t");

                    xmlWriter.WriteStartElement("src_frame_size");
                    xmlWriter.WriteAttributeString("value", "48");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteWhitespace("\r\n\t");

                    xmlWriter.WriteStartElement("position_x");
                    xmlWriter.WriteAttributeString("value", "48");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteWhitespace("\r\n\t");

                    xmlWriter.WriteStartElement("position_y");
                    xmlWriter.WriteAttributeString("value", "68");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteWhitespace("\r\n\t");

                    xmlWriter.WriteStartElement("hp");
                    xmlWriter.WriteAttributeString("value", "10");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteWhitespace("\r\n\t");

                    xmlWriter.WriteStartElement("gold");
                    xmlWriter.WriteAttributeString("value", "0");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteWhitespace("\r\n");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();
                    xmlWriter.Flush();
                    xmlWriter.Close();
                    #endregion
                }
            }
            catch (Exception error)
            {
                string methodName = System.Reflection.MethodBase.GetCurrentMethod().Name;
                Console.WriteLine(string.Format("An Error has occured in {0}.{1}, the Error message is: {2}", "Engine", methodName, error.Message));
            }
        }
    }
}