using System;
using System.Collections.Generic;
using System.Linq;
using LetsMoveIt.SelectionUtils;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace LetsMoveIt.Menus
{
    internal class ModMenu : IClickableMenu
    {
        private const int ItemsPerPage = 8;

        private readonly List<ClickableComponent> MenuSlots = [];
        private readonly List<SelectionElement> Options = [];
        public ClickableTextureComponent UpArrow;
        public ClickableTextureComponent DownArrow;
        public ClickableTextureComponent ScrollBar;
        private Rectangle ScrollBarRunner;
        private bool Scrolling;

        private int CurrentItemIndex;
        private string HoverText = "";

        public ModMenu() : base(Game1.uiViewport.Width / 2 - (800 + borderWidth * 2) / 2, Game1.uiViewport.Height / 2 - (600 + borderWidth * 2) / 2, 800 + borderWidth * 2, 600 + borderWidth * 2, true)
        {
            UpArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 459, 11, 12), 4f);
            DownArrow = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48), Game1.mouseCursors, new Rectangle(421, 472, 11, 12), 4f);
            ScrollBar = new ClickableTextureComponent(new Rectangle(UpArrow.bounds.X + 12, UpArrow.bounds.Y + UpArrow.bounds.Height + 4, 24, 40), Game1.mouseCursors, new Rectangle(435, 463, 6, 10), 4f);
            ScrollBarRunner = new Rectangle(ScrollBar.bounds.X, UpArrow.bounds.Y + UpArrow.bounds.Height + 4, ScrollBar.bounds.Width, height - 128 - UpArrow.bounds.Height - 8);

            CreateComponents();
            CurrentItemIndex = 0;

            if (Game1.activeClickableMenu == null)
            {
                Game1.playSound("bigSelect");
            }
        }

        public void Exit()
        {
            if (readyToClose() && !GameMenu.forcePreventClose)
            {
                exitThisMenu();
            }
        }

        public void Select(AreaSelection selectedArea)
        {
            if (Selection.TrySelect(selectedArea))
            {
                Exit();
                return;
            }
            Game1.playSound("cancel");
        }

        public void CreateComponents()
        {
            MenuSlots.Clear();
            Options.Clear();

            Rectangle bounds = new(0, 0, width - (borderWidth * 2) + 4, ((height - 128) / ItemsPerPage) - 8);

            for (int i = 0; i < ItemsPerPage; i++)
            {
                MenuSlots.Add(new ClickableComponent(new Rectangle(xPositionOnScreen + borderWidth - 4, yPositionOnScreen + 80 + 4 + i * ((height - 128) / ItemsPerPage) + 16, bounds.Width, bounds.Height), string.Concat(i))
                {
                    myID = i,
                    downNeighborID = i + 1,
                    upNeighborID = i - 1
                });
            }
            foreach (var selectedArea in Selection.AreaSelectionList.AsEnumerable().Reverse())
            {
                Options.Add(new SelectionElement(selectedArea, bounds, Select));
            }
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);

            UpArrow.bounds = new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + 64, 44, 48);
            DownArrow.bounds = new Rectangle(xPositionOnScreen + width + 16, yPositionOnScreen + height - 64, 44, 48);
            ScrollBar.bounds = new Rectangle(UpArrow.bounds.X + 12, UpArrow.bounds.Y + UpArrow.bounds.Height + 4, 24, 40);
            ScrollBarRunner = new Rectangle(ScrollBar.bounds.X, UpArrow.bounds.Y + UpArrow.bounds.Height + 4, ScrollBar.bounds.Width, height - 128 - UpArrow.bounds.Height - 8);
            SetScrollBarToCurrentIndex();
            initializeUpperRightCloseButton();

            for (int i = 0; i < MenuSlots.Count; i++)
            {
                MenuSlots[i].bounds.X = xPositionOnScreen + borderWidth - 4;
                MenuSlots[i].bounds.Y = yPositionOnScreen + 80 + 4 + i * ((height - 128) / ItemsPerPage) + 16;
            }
        }

        public void UpArrowPressed()
        {
            CurrentItemIndex--;
            UpArrow.scale = 3.5f;
            SetScrollBarToCurrentIndex();
        }

        public void DownArrowPressed()
        {
            CurrentItemIndex++;
            DownArrow.scale = 3.5f;
            SetScrollBarToCurrentIndex();
        }

        private void SetScrollBarToCurrentIndex()
        {
            if (Options.Count > 0)
            {
                ScrollBar.bounds.Y = ScrollBarRunner.Height / Math.Max(1, Options.Count - ItemsPerPage + 1) * CurrentItemIndex + UpArrow.bounds.Bottom + 4;
                if (CurrentItemIndex == Math.Max(0, Options.Count - ItemsPerPage))
                {
                    ScrollBar.bounds.Y = DownArrow.bounds.Y - ScrollBar.bounds.Height - 4;
                }
            }
        }

        public override void receiveScrollWheelAction(int direction)
        {
            base.receiveScrollWheelAction(direction);
            if (direction > 0 && CurrentItemIndex > 0)
            {
                UpArrowPressed();
                Game1.playSound("shiny4");
            }
            else if (direction < 0 && CurrentItemIndex < Math.Max(0, Options.Count - ItemsPerPage))
            {
                DownArrowPressed();
                Game1.playSound("shiny4");
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            if (Scrolling && Options.Count > ItemsPerPage)
            {
                int num = ScrollBar.bounds.Y;
                ScrollBar.bounds.Y = Math.Min(yPositionOnScreen + height - 64 - 12 - ScrollBar.bounds.Height, Math.Max(y, yPositionOnScreen + UpArrow.bounds.Height + 20));
                CurrentItemIndex = Math.Min(Options.Count - ItemsPerPage, Math.Max(0, (int)Math.Round((Options.Count - ItemsPerPage) * ((y - ScrollBarRunner.Y) / (float)ScrollBarRunner.Height))));
                SetScrollBarToCurrentIndex();
                if (num == ScrollBar.bounds.Y)
                    return;
                Game1.playSound("shiny4");
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            Scrolling = false;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y);
            if (UpArrow.containsPoint(x, y) && CurrentItemIndex > 0)
            {
                UpArrowPressed();
                Game1.playSound("shwip");
                return;
            }
            if (DownArrow.containsPoint(x, y) && CurrentItemIndex < Options.Count - ItemsPerPage)
            {
                DownArrowPressed();
                Game1.playSound("shwip");
                return;
            }
            if (ScrollBar.containsPoint(x, y))
            {
                Scrolling = true;
                return;
            }
            if (!DownArrow.containsPoint(x, y) && x > xPositionOnScreen + width && x < xPositionOnScreen + width + 128 && y > yPositionOnScreen && y < yPositionOnScreen + height)
            {
                Scrolling = true;
                leftClickHeld(x, y);
                releaseLeftClick(x, y);
                return;
            }
            for (int i = 0; i < MenuSlots.Count; i++)
            {
                if (MenuSlots[i].bounds.Contains(x, y) && CurrentItemIndex + i < Options.Count && Options[CurrentItemIndex + i].Bounds.Contains(x - MenuSlots[i].bounds.X, y - MenuSlots[i].bounds.Y))
                {
                    Options[CurrentItemIndex + i].receiveLeftClick(x - MenuSlots[i].bounds.X, y - MenuSlots[i].bounds.Y);
                    //Game1.playSound("shiny4");
                    break;
                }
            }
        }

        public override void performHoverAction(int x, int y)
        {
            HoverText = "";
            UpArrow.tryHover(x, y);
            DownArrow.tryHover(x, y);
            ScrollBar.tryHover(x, y);
            base.performHoverAction(x, y);
        }

        public override void draw(SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            }

            Game1.drawDialogueBox(xPositionOnScreen, yPositionOnScreen, width, height, false, true);

            b.End();
            b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp);
            // Draw Content

            for (int i = 0; i < MenuSlots.Count; i++)
            {
                if (CurrentItemIndex >= 0 && CurrentItemIndex + i < Options.Count)
                {
                    Options[CurrentItemIndex + i].draw(b, MenuSlots[i].bounds.X, MenuSlots[i].bounds.Y);
                }
                if (MenuSlots[i].bounds.Contains(Game1.getMouseX(), Game1.getMouseY()))
                {
                    b.Draw(Game1.staminaRect, new Rectangle(MenuSlots[i].bounds.X, MenuSlots[i].bounds.Y, MenuSlots[i].bounds.Width, MenuSlots[i].bounds.Height), Color.White * 0.25f);
                }
            }

            b.End();
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            for (int i = 1; i < MenuSlots.Count; i++)
            {
                drawHorizontalPartition(b, yPositionOnScreen + borderWidth + ((height - 128) / ItemsPerPage * i) + 24, small: true);
            }
            drawVerticalPartition(b, xPositionOnScreen + 320, small: true);

            UpArrow.draw(b);
            DownArrow.draw(b);
            drawTextureBox(b, Game1.mouseCursors, new Rectangle(403, 383, 6, 6), ScrollBarRunner.X, ScrollBarRunner.Y, ScrollBarRunner.Width, ScrollBarRunner.Height, Color.White, 4f);
            ScrollBar.draw(b);

            if (HoverText != "")
            {
                drawHoverText(b, HoverText, Game1.smallFont);
            }

            base.draw(b);
            drawMouse(b);
        }
    }
}
