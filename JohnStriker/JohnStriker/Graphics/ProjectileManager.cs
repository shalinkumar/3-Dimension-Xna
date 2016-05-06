using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace JohnStriker.Graphics
{
    public class ProjectileManager
    {
        GameManager Game;        // game manager

        // linked list of active projectiles
        LinkedList<Projectile> projectiles;

        // linked list of nodes to delete from the projectiles list
        List<LinkedListNode<Projectile>> deleteProjectiles;

        /// <summary>
        /// Create a new projetcile manager
        /// </summary>
        public ProjectileManager(GameManager game)
        {
            this.Game = game;

            projectiles = new LinkedList<Projectile>();
            deleteProjectiles = new List<LinkedListNode<Projectile>>();
        }

        /// <summary>
        /// Add a new projectile
        /// </summary>
        public void Add(Projectile p)
        {
            projectiles.AddLast(p);
        }

        // Missile1 3 1
        /// <summary>
        /// Update all projectiles
        /// </summary>
        public void Update(GameTime gameTime)
        {
            // empty deleted projectiles list
            deleteProjectiles.Clear();

            // for each powerup
            LinkedListNode<Projectile> Node = projectiles.First;
            while (Node != null)
            {
                // update projectile
                bool running = Node.Value.Update((float)gameTime.ElapsedGameTime.TotalSeconds, Game);

                // if finished running add to delete list
                if (running == false)
                    deleteProjectiles.Add(Node);

                // move to next node
                Node = Node.Next;
            }

            // delete all nodes in delete list
            foreach (LinkedListNode<Projectile> p in deleteProjectiles)
                projectiles.Remove(p);
        }

        // Missile1 2 1
        /// <summary>
        /// Draw all projectiles
        /// </summary>
        public void Draw(GraphicsDevice gd, RenderTechnique technique,
            Vector3 cameraPosition, Matrix viewProjection, LightList lights)
        {
            // draw all projectiles
            foreach (Projectile p in projectiles)
                p.Draw(Game, gd, technique, cameraPosition, viewProjection, lights);
        }
    }
}
