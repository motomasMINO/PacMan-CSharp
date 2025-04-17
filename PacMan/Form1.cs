using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;
using NAudio.Wave;

namespace PacMan
{
    public partial class Form1 : Form
    {
        // �����N���X Block�F�p�b�N�}���A�S�[�X�g�A�ǁA�G�T�Ȃǂ�\��
        public class Block
        {
            public int X;
            public int Y;
            public int Width;
            public int Height;
            public Image Image;
            public Image OriginalImage;

            public int StartX;
            public int StartY;
            public char Direction = 'U'; // U, D, L, R
            public int VelocityX = 0;
            public int VelocityY = 0;

            public bool IsScared = false; // �S�[�X�g���C�W�P��Ԃ��ǂ���

            // �R���X�g���N�^
            public Block(Image image, int x, int y, int width, int height)
            {
                this.Image = image;
                this.OriginalImage = image;
                this.X = x;
                this.Y = y;
                this.Width = width;
                this.Height = height;
                this.StartX = x;
                this.StartY = y;
            }

            // �����X�V�i�����ύX��ɑ��x�X�V���Փ˔���͊O���ŏ����j
            public void UpdateDirection(char newDirection, int tileSize, List<Block> walls)
            {
                char prevDirection = Direction;
                Direction = newDirection;
                UpdateVelocity(tileSize);

                // ���ړ�
                X += VelocityX;
                Y += VelocityY;

                // �ǂƂ̏Փ˔���i�Փ˂��Ă����猳�ɖ߂��j
                foreach (Block wall in walls)
                {
                    if (Collision(this, wall))
                    {
                        X -= VelocityX;
                        Y -= VelocityY;
                        Direction = prevDirection;
                        UpdateVelocity(tileSize);
                        break;
                    }
                }
            }

            // ���x�̍X�V
            public void UpdateVelocity(int tileSize)
            {
                if (Direction == 'U')
                {
                    VelocityX = 0;
                    VelocityY = -tileSize / 4;
                }
                else if (Direction == 'D')
                {
                    VelocityX = 0;
                    VelocityY = tileSize / 4;
                }
                else if (Direction == 'L')
                {
                    VelocityX = -tileSize / 4;
                    VelocityY = 0;
                }
                else if (Direction == 'R')
                {
                    VelocityX = tileSize / 4;
                    VelocityY = 0;
                }
            }

            public void Reset()
            {
                X = StartX;
                Y = StartY;
                IsScared = false;
                Image = OriginalImage;
            }

            // �ÓI�ȏՓ˔��胁�\�b�h
            public static bool Collision(Block a, Block b)
            {
                return a.X < b.X + b.Width &&
                       a.X + a.Width > b.X &&
                       a.Y < b.Y + b.Height &&
                       a.Y + a.Height > b.Y;
            }
        }

        // �萔�E�ϐ���`
        private int rowCount = 21;
        private int columnCount = 19;
        private int tileSize = 32;
        private int boardWidth;
        private int boardHeight;
        private int scaredGhostCount = 0;
        private int nextExtraLifeScore = 10000;
        private int eatenFoodCount = 0;
        private int eatenPowerFoodCount = 0;
        private int cherryScoreX = -1;
        private int cherryScoreY = -1;

        // �摜
        private Image wallImg;
        private Image powerFoodsImg;
        private Image blueGhostImg;
        private Image orangeGhostImg;
        private Image pinkGhostImg;
        private Image redGhostImg;
        private Image scaredGhostImg;
        private Image cherryImg;

        private Image pacmanUpImg;
        private Image pacmanDownImg;
        private Image pacmanLeftImg;
        private Image pacmanRightImg;

        // �T�E���h
        private Sound soundGameStart, soundGhostMoving, soundGhostScaring;

        // ���ʉ�
        private SoundEffect soundEat, soundLose, soundEatGhost, soundExtraLife, soundEatFruit;

        // �t�H���g
        private Font arcadeFont;

        // �Q�[���I�u�W�F�N�g
        private Block cherry = null;

        // �^�C�}�[
        private System.Windows.Forms.Timer cherryTimer;       // �`�F���[��������^�C�}�[
        private System.Windows.Forms.Timer cherryScoreTimer;
        private System.Windows.Forms.Timer gameLoopTimer;
        private System.Windows.Forms.Timer startDelayTimer;
        private System.Windows.Forms.Timer ghostScaredTimer;

        // �}�b�v
        private string[] tileMap = new string[]
        {
            "XXXXXXXXXXXXXXXXXXX",
            "X        X        X",
            "X@XX XXX X XXX XX@X",
            "X                 X",
            "X XX X XXXXX X XX X",
            "X    X       X    X",
            "XXXX XXXX XXXX XXXX",
            "OOOX X       X XOOO",
            "XXXX X XXrXX X XXXX",
            "O      XbpoX      O",
            "XXXX X XXXXX X XXXX",
            "OOOX X       X XOOO",
            "XXXX X XXXXX X XXXX",
            "X        X        X",
            "X XX XXX X XXX XX X",
            "X@ X     P     X @X",
            "XX X X XXXXX X X XX",
            "X    X   X   X    X",
            "X XXXXXX X XXXXXX X",
            "X                 X",
            "XXXXXXXXXXXXXXXXXXX"
        };

        // �Q�[�����̊e�u���b�N���X�g
        private List<Block> walls;
        private List<Block> foods;
        private List<Block> ghosts;
        private List<Block> powerFoods;
        private Block pacman;

        private char[] directions = new char[] { 'U', 'D', 'L', 'R' };
        private Random random = new Random();

        private int score = 0;
        private int lives = 3;
        private int rounds = 1;
        private bool gameOver = false;
        private bool gameStarted = false;

        public Form1()
        {
            boardWidth = columnCount * tileSize;
            boardHeight = rowCount * tileSize;
            this.ClientSize = new Size(boardWidth, boardHeight);
            this.Text = "PAC-MAN";
            this.DoubleBuffered = true;
            this.BackColor = Color.Black;

            this.KeyPreview = true;
            this.KeyDown += PacManForm_KeyDown;

            // �摜�ǂݍ��݁i
            wallImg = Image.FromFile("Resources/wall.png");
            powerFoodsImg = Image.FromFile("Resources/powerFood.png");
            blueGhostImg = Image.FromFile("Resources/blueGhost.png");
            orangeGhostImg = Image.FromFile("Resources/orangeGhost.png");
            pinkGhostImg = Image.FromFile("Resources/pinkGhost.png");
            redGhostImg = Image.FromFile("Resources/redGhost.png");
            scaredGhostImg = Image.FromFile("Resources/scaredGhost.png");
            cherryImg = Image.FromFile("Resources/cherry.png");

            pacmanUpImg = Image.FromFile("Resources/pacmanUp.png");
            pacmanDownImg = Image.FromFile("Resources/pacmanDown.png");
            pacmanLeftImg = Image.FromFile("Resources/pacmanLeft.png");
            pacmanRightImg = Image.FromFile("Resources/pacmanRight.png");

            // �T�E���h�ǂݍ���
            soundEat = new SoundEffect("Resources/Pacman_Eat.wav");
            soundLose = new SoundEffect("Resources/Pacman_Lose.wav");
            soundGameStart = new Sound("Resources/gameStart.wav");
            soundGhostMoving = new Sound("Resources/ghostMoving.wav");
            soundGhostScaring = new Sound("Resources/ghostScaring.wav");
            soundEatGhost = new SoundEffect("Resources/eatGhost.wav");
            soundExtraLife = new SoundEffect("Resources/Extend.wav");
            soundEatFruit = new SoundEffect("Resources/Eat-Fruit.wav");

            // �t�H���g
            arcadeFont = new Font("Arial", 18, FontStyle.Bold);

            LoadMap();

            // �S�[�X�g�̏��������ݒ�
            foreach (Block ghost in ghosts)
            {
                char newDirection = directions[random.Next(4)];
                ghost.UpdateDirection(newDirection, tileSize, walls);
            }

            // �Q�[�����[�v�^�C�}�[ (50ms���ƂɍX�V����20fps)
            gameLoopTimer = new System.Windows.Forms.Timer();
            gameLoopTimer.Interval = 50;
            gameLoopTimer.Tick += GameLoopTimer_Tick;

            // �x���J�n�^�C�}�[�i5.5�b��ɃQ�[���J�n�j
            startDelayTimer = new System.Windows.Forms.Timer();
            startDelayTimer.Interval = 5500;
            startDelayTimer.Tick += (s, e) =>
            {
                gameStarted = true;
                gameLoopTimer.Start();
                // �T�E���h�̃��[�v�Đ�
                // �����ł͒P���ɍĐ�
                soundGhostMoving.Loop();
                startDelayTimer.Stop();
            };
            startDelayTimer.Start();
            soundGameStart.Play();

            // �`�F���[�^�C�}�[�i10�b��Ƀ`�F���[�������j
            cherryTimer = new System.Windows.Forms.Timer();
            cherryTimer.Interval = 10000;
            cherryTimer.Tick += (s, e) =>
            {
                cherry = null;
                cherryTimer.Stop();
            };
        }

        // �}�b�v�̓ǂݍ���
        private void LoadMap()
        {
            walls = new List<Block>();
            foods = new List<Block>();
            powerFoods = new List<Block>();
            ghosts = new List<Block>();

            for (int r = 0; r < rowCount; r++)
            {
                string row = tileMap[r];
                for (int c = 0; c < columnCount; c++)
                {
                    char tileChar = row[c];
                    int x = c * tileSize;
                    int y = r * tileSize;

                    if (tileChar == 'X')
                    {
                        // ��
                        Block wall = new Block(wallImg, x, y, tileSize, tileSize);
                        walls.Add(wall);
                    }
                    else if (tileChar == 'b')
                    {
                        // �C���L�[
                        Block ghost = new Block(blueGhostImg, x, y, tileSize, tileSize);
                        ghosts.Add(ghost);
                    }
                    else if (tileChar == 'o')
                    {
                        // �N���C�h
                        Block ghost = new Block(orangeGhostImg, x, y, tileSize, tileSize);
                        ghosts.Add(ghost);
                    }
                    else if (tileChar == 'p')
                    {
                        // �s���L�[
                        Block ghost = new Block(pinkGhostImg, x, y, tileSize, tileSize);
                        ghosts.Add(ghost);
                    }
                    else if (tileChar == 'r')
                    {
                        // �u�����L�[
                        Block ghost = new Block(redGhostImg, x, y, tileSize, tileSize);
                        ghosts.Add(ghost);
                    }
                    else if (tileChar == 'P')
                    {
                        // �p�b�N�}��
                        pacman = new Block(pacmanLeftImg, x, y, tileSize, tileSize);
                    }
                    else if (tileChar == ' ')
                    {
                        // �G�T�F�T�C�Y�͒����ɏ����ȋ�`
                        Block food = new Block(null, x + 14, y + 14, 4, 4);
                        foods.Add(food);
                    }
                    else if (tileChar == '@')
                    {
                        // �p���[�G�T�F�T�C�Y�͒����ɑ傫�ȋ�`
                        Block powerFood = new Block(powerFoodsImg, x + 10, y + 10, 12, 12);
                        powerFoods.Add(powerFood);
                    }
                }
            }
        }

        // �Q�[�����[�v��Tick�C�x���g
        private void GameLoopTimer_Tick(object sender, EventArgs e)
        {
            if (!gameStarted)
                return;

            Move();
            Invalidate();

            if (gameOver)
            {
                gameLoopTimer.Stop();
                soundGhostMoving.Stop();
                soundGhostScaring.Stop();
                cherryTimer.Stop();
            }
        }

        // �Q�[�����̈ړ�����
        private void Move()
        {
            // �p�b�N�}���̈ړ�
            pacman.X += pacman.VelocityX;
            pacman.Y += pacman.VelocityY;

            // ��ʊO�̏ꍇ�A���Α�����o��������
            if (pacman.X < 0)
            {
                pacman.X = boardWidth - pacman.Width;
            }
            else if (pacman.X + pacman.Width > boardWidth)
            {
                pacman.X = 0;
            }
            if (pacman.Y < 0)
            {
                pacman.Y = boardHeight - pacman.Height;
            }
            else if (pacman.Y + pacman.Height > boardHeight)
            {
                pacman.Y = 0;
            }

            // �ǂƂ̓����蔻��
            foreach (Block wall in walls)
            {
                if (Block.Collision(pacman, wall))
                {
                    pacman.X -= pacman.VelocityX;
                    pacman.Y -= pacman.VelocityY;
                    break;
                }
            }

            // �S�[�X�g�̈ړ�����уp�b�N�}���Ƃ̏Փ˔���
            foreach (Block ghost in ghosts)
            {
                if (Block.Collision(ghost, pacman))
                {
                    if (ghost.IsScared)
                    {
                        score += 200;
                        soundEatGhost.Play();

                        if (score >= nextExtraLifeScore)
                        {
                            lives++;
                            nextExtraLifeScore += 10000;
                            soundExtraLife.Play();
                        }

                        ghost.Reset();
                        scaredGhostCount--;
                        ghost.IsScared = false;

                        if (scaredGhostCount <= 0)
                        {
                            soundGhostScaring.Stop();
                            soundGhostMoving.Loop();
                        }
                    }
                    else
                    {
                        lives--;
                        cherry = null;
                        cherryTimer.Stop();
                        soundLose.Play();
                        soundGhostScaring.Stop();
                        soundGhostMoving.Loop();

                        if (lives == 0)
                        {
                            gameOver = true;
                            return;
                        }
                        ResetPositions();
                    }
                }

                // �S�[�X�g�̈ړ�����
                // ��������ŕ����ύX�i��F���������Ɉړ�����j
                if (ghost.Y == tileSize * 9 && ghost.Direction != 'U' && ghost.Direction != 'D')
                {
                    ghost.UpdateDirection('U', tileSize, walls);
                }

                ghost.X += ghost.VelocityX;
                ghost.Y += ghost.VelocityY;
                bool collided = false;
                if (ghost.X <= 0 || ghost.X + ghost.Width >= boardWidth)
                    collided = true;
                foreach (Block wall in walls)
                {
                    if (Block.Collision(ghost, wall))
                    {
                        collided = true;
                        break;
                    }
                }
                if (collided)
                {
                    ghost.X -= ghost.VelocityX;
                    ghost.Y -= ghost.VelocityY;
                    char newDirection = directions[random.Next(4)];
                    ghost.UpdateDirection(newDirection, tileSize, walls);
                }
            }

            // �G�T�Ƃ̓����蔻��
            Block foodEaten = null;
            foreach (Block food in foods)
            {
                if (Block.Collision(pacman, food))
                {
                    foodEaten = food;
                    score += 10;
                    eatenFoodCount++;
                    soundEat.Play();

                    // �`�F���[�o������
                    if (eatenFoodCount == 70 || eatenFoodCount == 170)
                    {
                        SpawnCherry();
                    }

                    if (score >= nextExtraLifeScore)
                    {
                        lives++;
                        nextExtraLifeScore += 10000;
                        soundExtraLife.Play();
                    }
                    break;
                }
            }
            if (foodEaten != null)
            {
                foods.Remove(foodEaten);
            }

            // �`�F���[�Ƃ̓����蔻��
            if (cherry != null && Block.Collision(pacman, cherry))
            {
                score += 100;
                cherryScoreX = boardWidth / 2 - 20;
                cherryScoreY = boardHeight / 2 + 40;

                if (cherryScoreTimer != null)
                    cherryScoreTimer.Stop();
                cherryScoreTimer = new System.Windows.Forms.Timer();
                cherryScoreTimer.Interval = 1500;
                cherryScoreTimer.Tick += (s, e) =>
                {
                    cherryScoreX = -1;
                    cherryScoreY = -1;
                    Invalidate();
                    cherryScoreTimer.Stop();
                };
                cherryScoreTimer.Start();

                cherry = null;
                cherryTimer.Stop();
                soundEatFruit.Play();
            }

            // �p���[�G�T�Ƃ̓����蔻��
            Block powerFoodEaten = null;
            foreach (Block powerFood in powerFoods)
            {
                if (Block.Collision(pacman, powerFood))
                {
                    powerFoodEaten = powerFood;
                    score += 50;
                    eatenPowerFoodCount++;
                    soundEat.Play();

                    if (score >= nextExtraLifeScore)
                    {
                        lives++;
                        nextExtraLifeScore += 10000;
                        soundExtraLife.Play();
                    }

                    // ���ׂẴS�[�X�g���C�W�P��Ԃɂ���
                    scaredGhostCount = ghosts.Count;
                    foreach (Block ghost in ghosts)
                    {
                        ghost.IsScared = true;
                        ghost.Image = scaredGhostImg;
                    }
                    soundGhostMoving.Stop();
                    soundGhostScaring.Loop();

                    if (ghostScaredTimer != null)
                        ghostScaredTimer.Stop();
                    ghostScaredTimer = new System.Windows.Forms.Timer();
                    ghostScaredTimer.Interval = 10000;
                    ghostScaredTimer.Tick += (s, e) =>
                    {
                        foreach (Block ghost in ghosts)
                        {
                            ghost.IsScared = false;
                            ghost.Image = ghost.OriginalImage;
                            scaredGhostCount--;
                        }
                        ghostScaredTimer.Stop();
                        soundGhostScaring.Stop();
                        soundGhostMoving.Loop();
                    };
                    ghostScaredTimer.Start();
                    break;
                }
            }
            if (powerFoodEaten != null)
            {
                powerFoods.Remove(powerFoodEaten);
            }

            // �G�T���S�ĂȂ��Ȃ����ꍇ�F�}�b�v���Z�b�g
            if (foods.Count == 0 && powerFoods.Count == 0)
            {
                cherry = null;
                LoadMap();
                ResetPositions();
                rounds++;
                eatenFoodCount = 0;
                eatenPowerFoodCount = 0;
                soundGhostScaring.Stop();
                soundGhostMoving.Loop();
            }
        }

        // �`�F���[���o��������
        private void SpawnCherry()
        {
            int cherryX = boardWidth / 2 - tileSize / 2;
            int cherryY = boardHeight / 2 - tileSize / 2 + 30;
            cherry = new Block(cherryImg, cherryX, cherryY, tileSize, tileSize);
            cherryScoreX = -5;
            cherryScoreY = -5;
            cherryTimer.Start();
        }

        // �S�L�����N�^�[�̈ʒu���Z�b�g
        private void ResetPositions()
        {
            pacman.Reset();
            pacman.VelocityX = 0;
            pacman.VelocityY = 0;
            foreach (Block ghost in ghosts)
            {
                ghost.Reset();
                char newDirection = directions[random.Next(4)];
                ghost.UpdateDirection(newDirection, tileSize, walls);
            }
        }

        // �`�揈��
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;

            // �p�b�N�}���`��
            if (pacman.Image != null)
            {
                g.DrawImage(pacman.Image, pacman.X, pacman.Y, pacman.Width, pacman.Height);
            }

            // �S�[�X�g�`��
            foreach (Block ghost in ghosts)
            {
                if (ghost.Image != null)
                {
                    g.DrawImage(ghost.Image, ghost.X, ghost.Y, ghost.Width, ghost.Height);
                }
            }

            // �Ǖ`��
            foreach (Block wall in walls)
            {
                if (wall.Image != null)
                {
                    g.DrawImage(wall.Image, wall.X, wall.Y, wall.Width, wall.Height);
                }
            }

            // �G�T�`��
            using (Brush brush = new SolidBrush(Color.White))
            {
                foreach (Block food in foods)
                {
                    g.FillRectangle(brush, food.X, food.Y, food.Width, food.Height);
                }
            }

            // �p���[�G�T�`��
            foreach (Block powerFood in powerFoods)
            {
                if (powerFood.Image != null)
                {
                    g.DrawImage(powerFood.Image, powerFood.X, powerFood.Y, powerFood.Width, powerFood.Height);
                }
            }

            // �`�F���[�`��
            if (cherry != null && cherry.Image != null)
            {
                g.DrawImage(cherry.Image, cherry.X, cherry.Y, cherry.Width, cherry.Height);
            }

            // �X�R�A�A�c�@�A���E���h�̕\��
            if(gameOver)
            {
                g.DrawString("GAME OVER: " + score.ToString(), arcadeFont, Brushes.Red, tileSize / 2, tileSize / 2 - 20);
            }else
            {
                g.DrawString("LIFE x " + lives.ToString() + " SCORE: " + score.ToString(), arcadeFont, Brushes.White, tileSize / 2, tileSize / 2 - 20);
            }
           
            g.DrawString("ROUND " + rounds.ToString(), arcadeFont, Brushes.White, 450, tileSize / 2 - 20);

            // �`�F���[���_�\��
            if (cherryScoreX != -1 && cherryScoreY != -1)
            {
                g.DrawString("100", arcadeFont, Brushes.Pink, cherryScoreX - 10, cherryScoreY - 25);
            }

            // �Q�[���J�n�O�̕\��
            if (!gameStarted)
            {
                g.DrawString("READY!", arcadeFont, Brushes.Yellow, boardWidth / 2 - 60, boardHeight / 2 + 15);
            }
        }

        // �L�[���͏���
        private void PacManForm_KeyDown(object sender, KeyEventArgs e)
        {
            // �Q�[���I�[�o�[���A�C�ӂ̃L�[�ōăX�^�[�g
            if (gameOver)
            {
                LoadMap();
                ResetPositions();
                eatenFoodCount = 0;
                eatenPowerFoodCount = 0;
                lives = 3;
                score = 0;
                rounds = 1;
                gameOver = false;
                gameStarted = false;
                Invalidate();

                if (soundGhostMoving != null)
                    soundGhostMoving.Stop();
                startDelayTimer.Start();
                return;
            }

            // �p�b�N�}������
            if (e.KeyCode == Keys.Up)
            {
                pacman.UpdateDirection('U', tileSize, walls);
                pacman.Image = pacmanUpImg;
            }
            else if (e.KeyCode == Keys.Down)
            {
                pacman.UpdateDirection('D', tileSize, walls);
                pacman.Image = pacmanDownImg;
            }
            else if (e.KeyCode == Keys.Left)
            {
                pacman.UpdateDirection('L', tileSize, walls);
                pacman.Image = pacmanLeftImg;
            }
            else if (e.KeyCode == Keys.Right)
            {
                pacman.UpdateDirection('R', tileSize, walls);
                pacman.Image = pacmanRightImg;
            }
        }
    }
}