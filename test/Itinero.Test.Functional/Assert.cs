using System;

namespace Itinero.Test.Functional
{
    public static class Assert
    {
        /// <summary>
        /// Tests if the given value is true.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="message"></param>
        /// <exception cref="Exception"></exception>
        public static void IsTrue(bool val, string message = "Was false, expected true.")
        {
            if (!val)
            {
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Tests if the given value is not null.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="message"></param>
        /// <exception cref="Exception"></exception>
        public static void IsNotNull(object val, string message = "Was null, expected an non-null object.")
        {
            if (val == null)
            {
                throw new Exception(message);
            }
        }
    }
}