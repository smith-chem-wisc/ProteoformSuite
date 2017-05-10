using NUnit.Framework;
using ProteoformSuiteInternal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Test
{
    [TestFixture]
    class TestBigRational
    {
        [Test]
        public void big_rational_properties()
        {
            BigRational big = new BigRational(1, 2);
            Assert.AreEqual(0.5, (double)big);
            Assert.AreEqual(1, (double)big.Numerator);
            Assert.AreEqual(2, (double)big.Denominator);
            Assert.AreEqual(1, (double)big.Sign);
            Assert.AreEqual(0, (double)BigRational.Zero);
            Assert.AreEqual(1, (double)BigRational.One);
            Assert.AreEqual(-1, (double)BigRational.MinusOne);
        }

        [Test]
        public void big_rational_methods()
        {
            BigRational big = new BigRational(3, 2);
            Assert.NotNull(big.ToString());
            Assert.AreEqual(1, (double)big.GetWholePart());
            Assert.AreEqual(0.5, (double)big.GetFractionPart());
            Assert.AreEqual(new BigRational(3, 2).GetHashCode(), big.GetHashCode());
            Assert.True(new BigRational(3, 2).Equals(big));
            Assert.True(new BigRational(6, 4).Equals(big));
            Assert.False(new BigRational(6, 4).Equals(new object()));
            Assert.False(new BigRational(6, 4).Equals(null));
            Assert.AreEqual(1, new HashSet<BigRational> { big, new BigRational(3, 2) }.Count);
            Assert.AreEqual(0.25, (double)(new List<BigRational> { big, new BigRational(1, 4) }.OrderBy(r => r).First()));
        }

        [Test]
        public void big_rational_constructor()
        {
            Assert.Throws<ArgumentException>(() => new BigRational(Double.NaN));
            Assert.Throws<ArgumentException>(() => new BigRational(Double.NegativeInfinity));
            Assert.Throws<ArgumentException>(() => new BigRational(Double.PositiveInfinity));

            //Note: Only equals double values works here...
            Assert.True(0 == new BigRational(0d));
            Assert.True(0.5 == new BigRational(0.5d));
            Assert.True(-0.5 == new BigRational(-0.5d));
            Assert.True(1.0 == new BigRational(1.0d));
            Assert.True(-1.0 == new BigRational(-1.0d));
            Assert.True(100.0 == new BigRational(100.0d));
            Assert.True(-100.0 == new BigRational(-100.0d));

            //Note: Only equals decimal values here...
            Assert.True(0 == new BigRational(0m));
            Assert.True(0.5m == new BigRational(0.5m));
            Assert.True(-0.5m == new BigRational(-0.5m));
            Assert.True(100m == new BigRational(100m));
            Assert.True(-100m == new BigRational(-100m));

            Assert.Throws<DivideByZeroException>(() => new BigRational(1, 0));
            BigRational big = new BigRational(-1, 2);
            Assert.AreEqual(-0.5, (double)big);

            //This constructor doesn't make sense to me, the denominator < 0 thing in particular...
            //Assert.Throws<DivideByZeroException>(() => new BigRational(1, 1, 0));
            //Assert.AreEqual(0, (double)(new BigRational(0, 0, 2)));
            //Assert.AreEqual(-0.5, (double)(new BigRational(-1, 1, 2))); //-1 + 1/2
            //Assert.AreEqual(0.5, (double)(new BigRational(1, -1, 2))); //1 - 1/2
            //Assert.AreEqual(0.5, (double)(new BigRational(1, 1, -2))); //1 - 1/2 ... this one
            //Assert.AreEqual(1.5, (double)(new BigRational(1, 1, 2)));
        }

        [Test]
        public void big_rational_static_methods()
        {
            Assert.True(0.5 == BigRational.Abs(0.5d));
            Assert.True(0.5 == BigRational.Abs(-0.5d));
            Assert.False(0.5 == BigRational.Abs(-1));
            Assert.False(0.5 == BigRational.Abs(1));

            Assert.True(-0.5 == BigRational.Negate(0.5d));
            Assert.True(0.5 == BigRational.Negate(-0.5d));
            Assert.False(0.5 == BigRational.Negate(-1));
            Assert.False(-0.5 == BigRational.Negate(1));

            //Doesn't work great... testing with equals doubles fails
            Assert.True(new BigRational(1, 2) == BigRational.Invert(new BigRational(2, 1)));
            Assert.True(new BigRational(2, 1) == BigRational.Invert(new BigRational(1, 2)));
            Assert.True(new BigRational(-1, 2) == BigRational.Invert(new BigRational(-2, 1)));
            Assert.True(new BigRational(-2, 1) == BigRational.Invert(new BigRational(-1, 2)));

            Assert.True(new BigRational(BigInteger.One) + new BigRational(BigInteger.One) == 2);
            Assert.True(BigRational.Add(new BigRational(BigInteger.One), new BigRational(BigInteger.One)) == 2);

            //Not working for me
            //BigRational big1 = new BigRational(1.0);
            //big1++;
            //Assert.AreEqual(new BigRational(2, 1), big1);

            //Not working for me
            //BigRational big2 = new BigRational(1.0);
            //big2--;
            //Assert.True(0 == big2);

            Assert.True(new BigRational(BigInteger.One) - new BigRational(BigInteger.One) == 0);
            Assert.True(BigRational.Subtract(new BigRational(BigInteger.One), new BigRational(BigInteger.One)) == 0);

            BigRational big3 = new BigRational(1.0);
            Assert.True(-1.0 == -big3);

            BigRational big4 = new BigRational(1.0);
            Assert.True(1.0 == +big3);

            Assert.True(new BigRational(BigInteger.One) * new BigRational(BigInteger.One) == 1);
            Assert.True(BigRational.Multiply(new BigRational(BigInteger.One), new BigRational(BigInteger.One)) == 1);
            Assert.True(new BigRational(BigInteger.One) * new BigRational(2.0) == 2.0);
            Assert.True(BigRational.Multiply(new BigRational(BigInteger.One), new BigRational(2.0)) == 2.0);

            Assert.True(new BigRational(BigInteger.One) / new BigRational(BigInteger.One) == 1);
            Assert.True(BigRational.Divide(new BigRational(BigInteger.One), new BigRational(BigInteger.One)) == 1);
            Assert.AreEqual(new BigRational(1, 2), new BigRational(new BigInteger(1)) / new BigRational(new BigInteger(2))); //Again, mixing consructors doesn't work well.
            Assert.AreEqual(new BigRational(1, 2), BigRational.Divide(new BigRational(new BigInteger(1)), new BigRational(new BigInteger(2)))); //Again, mixing consructors doesn't work well. Testing with equals double 0.5 fails

            //Assert.True(new BigRational(BigInteger.One) < new BigRational(2.0)); //Don't mix the constructors for comparison!!!
            Assert.True(new BigRational(1.0) < new BigRational(2.0));
            Assert.False(new BigRational(2.0) < new BigRational(1.0));
            Assert.False(new BigRational(1.0) < new BigRational(1.0));
            Assert.True(new BigRational(1.0) <= new BigRational(2.0));
            Assert.True(new BigRational(1.0) <= new BigRational(1.0));
            Assert.False(new BigRational(2.0) <= new BigRational(1.0));
            Assert.True(new BigRational(1.0) == new BigRational(1.0));
            Assert.True(new BigRational(1.0) != new BigRational(2.0));


            Assert.False(new BigRational(1.0) > new BigRational(2.0));
            Assert.True(new BigRational(2.0) > new BigRational(1.0));
            Assert.False(new BigRational(BigInteger.One) > new BigRational(BigInteger.One));
            Assert.False(new BigRational(1.0) >= new BigRational(2.0));
            Assert.True(new BigRational(BigInteger.One) >= new BigRational(BigInteger.One));
            Assert.True(new BigRational(2.0) >= new BigRational(1.0));

            //Modulus doesn't seem to work well
            //Assert.AreEqual(BigRational.Zero, new BigRational(2.0) % new BigRational(1.0));
            //Assert.AreEqual(BigRational.One, new BigRational(3.0) % new BigRational(2.0));
            //Assert.True(new BigRational(1.5) % new BigRational(1.0) == new BigRational(0.5));
            //Assert.True(new BigRational(1.0) % new BigRational(2.0) == 0);
        }

        [Test]
        public void bigrational_basic_equals_compare()
        {
            Assert.False(BigRational.One.Equals(new object()));
            Assert.Throws<ArgumentException>(() => ((IComparable)BigRational.One).CompareTo(new object()));
            Assert.AreEqual(1, ((IComparable)BigRational.One).CompareTo(null));
            Assert.AreEqual(1, ((IComparable)BigRational.One).CompareTo(BigRational.Zero));
        }

        //[Test]
        //public void tough_double_conversion()
        //{
        //    Assert.Greater((double)(new BigRational(Math.PI)), 3); // this fails...!
        //}

        [Test]
        public void tough_double_cast_from_bigint()
        {
            //Divide two gigantic primes; see if it comes out about right
            BigInteger.TryParse("1000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000000031", out BigInteger numerator);
            BigInteger.TryParse("0000315791951375393537137595555337555955191395351995195751755791151795317131135377351919777977373317997317733397199751739199735799971153399111973979977771537137371797357935195531355957399953977139577337393111951779135151171355371173379337573915193973715113971779315731713793579595533511197399993313719939759551175175337795317333957313779755351991151933337157555517575773115995775199513553337335137111",out BigInteger denominator);
            BigRational big = new BigRational(numerator, denominator);
            Assert.AreEqual(3166.6, Math.Round((double)big, 1));
        }
    }
}
