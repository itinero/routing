This fails:
        
            // another case.
            var c1 = new Coordinate(51.27015181278037f, 4.799544811248779f);
            var c2 = new Coordinate(51.268896560564926f, 4.798074960708618f);
            var c3 = new Coordinate(51.268970399879706f, 4.800456762313843f);

            p = new Polygon();
            p.ExteriorRing.Add(c1);
            p.ExteriorRing.Add(c2);
            p.ExteriorRing.Add(c3);
            p.ExteriorRing.Add(c1);

            var c4 = new Coordinate(51.26966179861588f, 4.799458980560303f);
            var c5 = new Coordinate(51.26915164133116f, 4.798922538757324f);
            var c6 = new Coordinate(51.269158353963775f, 4.799877405166626f);

            p.InteriorRings.Add(new List<Coordinate>());
            p.InteriorRings[0].Add(c4);
            p.InteriorRings[0].Add(c5);
            p.InteriorRings[0].Add(c6);
            p.InteriorRings[0].Add(c4);

            // this should be 'in the hole'.
            Assert.IsFalse(p.PointIn(new Coordinate(51.269279181183315f, 4.799507260322571f)));