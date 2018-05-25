/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.LocalGeo;

namespace Itinero.Navigation.Directions
{
    /// <summary>
    /// Calculates direction.
    /// </summary>
    public static class DirectionCalculator
    {
        /// <summary>
        /// Calculates the angle in randians at coordinate2.
        /// </summary>
        public static float Angle(Coordinate coordinate1, Coordinate coordinate2, Coordinate coordinate3)
        {
            var v11 = coordinate1.Latitude - coordinate2.Latitude;
            var v10 = coordinate1.Longitude - coordinate2.Longitude;

            var v21 = coordinate3.Latitude - coordinate2.Latitude;
            var v20 = coordinate3.Longitude - coordinate2.Longitude;

            var v1size = System.Math.Sqrt(v11 * v11 + v10 * v10);
            var v2size = System.Math.Sqrt(v21 * v21 + v20 * v20);

            if (v1size == 0 || v2size == 0)
            {
                return float.NaN;
            }

            // filter out the vectors that are parallel.
            if (v10 == v20 && 
                v11 == v21)
            {
                return 0;
            }
            else if (v10 == v20 && 
                v11 == -v21)
            {
                return (float)(System.Math.PI);
            }
            else if (v10 == -v20 &&
                v11 == v21)
            {
                return (float)(-System.Math.PI);
            }
            else if (v10 == -v20 &&
                v11 == -v21)
            {
                return (float)(2 * System.Math.PI);
            }

            var dot = (double)(v11 * v21 + v10 * v20);
            var cross = (double)(v10 * v21 - v11 * v20);

            // split per quadrant.
            double angle;
            if (dot > 0)
            { // dot > 0
                if (cross > 0)
                { // dot > 0 and cross > 0
                    // Quadrant 1
                    angle = (double)System.Math.Asin(cross / (v1size * v2size));
                    if (angle < System.Math.PI / 4f)
                    { // use cosine.
                        angle = (double)System.Math.Acos(dot / (v1size * v2size));
                    }
                    // angle is ok here for quadrant 1.
                }
                else
                { // dot > 0 and cross <= 0
                    // Quadrant 4
                    angle = (double)(System.Math.PI * 2.0f) + (double)System.Math.Asin(cross / (v1size * v2size));
                    if (angle > (double)(System.Math.PI * 2.0f) - System.Math.PI / 4f)
                    { // use cosine.
                        angle = (double)(System.Math.PI * 2.0f) - (double)System.Math.Acos(dot / (v1size * v2size));
                    }
                    // angle is ok here for quadrant 1.
                }
            }
            else
            { // dot <= 0
                if (cross > 0)
                { // dot > 0 and cross > 0
                    // Quadrant 2
                    angle = (double)System.Math.PI - (double)System.Math.Asin(cross / (v1size * v2size));
                    if (angle > System.Math.PI / 2f + System.Math.PI / 4f)
                    { // use cosine.
                        angle = (double)System.Math.Acos(dot / (v1size * v2size));
                    }
                    // angle is ok here for quadrant 2.
                }
                else
                { // dot > 0 and cross <= 0
                    // Quadrant 3
                    angle = -(-(double)System.Math.PI + (double)System.Math.Asin(cross / (v1size * v2size)));
                    if (angle < System.Math.PI + System.Math.PI / 4f)
                    { // use cosine.
                        angle = (double)(System.Math.PI * 2.0f) - (double)System.Math.Acos(dot / (v1size * v2size));
                    }
                    // angle is ok here for quadrant 3.
                }
            }
            return (float)angle;
        }

        /// <summary>
        /// Calculates the direction of one line segment relative to another.
        /// </summary>
        public static RelativeDirection Calculate(Coordinate coordinate1, Coordinate coordinate2, Coordinate coordinate3)
        {
            var direction = new RelativeDirection();

            var margin = 65.0;
            var straightOn = 10.0;
            var turnBack = 5.0;

            var angleRandians = DirectionCalculator.Angle(coordinate1, coordinate2, coordinate3);
            var angle = angleRandians.ToDegrees();

            angle = angle.NormalizeDegrees();

            if (angle >= 360 - turnBack || angle < turnBack)
            {
                direction.Direction = RelativeDirectionEnum.TurnBack;
            }
            else if (angle >= turnBack && angle < 90 - margin)
            {
                direction.Direction = RelativeDirectionEnum.SharpRight;
            }
            else if (angle >= 90 - margin && angle < 90 + margin)
            {
                direction.Direction = RelativeDirectionEnum.Right;
            }
            else if (angle >= 90 + margin && angle < 180 - straightOn)
            {
                direction.Direction = RelativeDirectionEnum.SlightlyRight;
            }
            else if (angle >= 180 - straightOn && angle < 180 + straightOn)
            {
                direction.Direction = RelativeDirectionEnum.StraightOn;
            }
            else if (angle >= 180 + straightOn && angle < 270 - margin)
            {
                direction.Direction = RelativeDirectionEnum.SlightlyLeft;
            }
            else if (angle >= 270 - margin && angle < 270 + margin)
            {
                direction.Direction = RelativeDirectionEnum.Left;
            }
            else if (angle >= 270 + margin && angle < 360 - turnBack)
            {
                direction.Direction = RelativeDirectionEnum.SharpLeft;
            }
            direction.Angle = (float)angle;

            return direction;
        }

        /// <summary>
        /// Calculates the direction of a segment.
        /// </summary>
        public static DirectionEnum Calculate(Coordinate coordinate1, Coordinate coordinate2)
        {
            var angle = (double)DirectionCalculator.Angle(new Coordinate(coordinate1.Latitude + 0.01f, coordinate1.Longitude),
                coordinate1, coordinate2);

            angle = angle.ToDegrees();
            angle = angle.NormalizeDegrees();

            if (angle < 22.5 || angle >= 360 - 22.5)
            { // north
                return DirectionEnum.North;
            }
            else if (angle >= 22.5 && angle < 90 - 22.5)
            { // north-east.
                return DirectionEnum.NorthWest;
            }
            else if (angle >= 90 - 22.5 && angle < 90 + 22.5)
            { // east.
                return DirectionEnum.West;
            }
            else if (angle >= 90 + 22.5 && angle < 180 - 22.5)
            { // south-east.
                return DirectionEnum.SouthWest;
            }
            else if (angle >= 180 - 22.5 && angle < 180 + 22.5)
            { // south
                return DirectionEnum.South;
            }
            else if (angle >= 180 + 22.5 && angle < 270 - 22.5)
            { // south-west.
                return DirectionEnum.SouthEast;
            }
            else if (angle >= 270 - 22.5 && angle < 270 + 22.5)
            { // south-west.
                return DirectionEnum.East;
            }
            else if (angle >= 270 + 22.5 && angle < 360 - 22.5)
            { // south-west.
                return DirectionEnum.NorthEast;
            }
            return DirectionEnum.North;
        }
    }
}