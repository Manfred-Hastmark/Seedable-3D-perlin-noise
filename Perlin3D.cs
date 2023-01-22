using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for generating seedable 3D perlin noise.
/// <br/> Has support for <see href="https://unity.com/">Unity</see> <see cref="Vector3"/>
/// <br/>
/// <br/>Sources and extra reading material on how perlin noise works:
/// <list type="bullet">
/// <item><description><see href="https://weber.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf">Simplex noise demystified</see></description></item>
/// <item><description><see href="https://github.com/keijiro/PerlinNoise"/></description></item>
/// <item><description><see href="https://www.youtube.com/watch?v=ip0XBBBY6A8">Interpolation</see> by <see href="https://www.youtube.com/@mathsgenie7808">Maths Genie</see></description></item>
/// <item><description><see href="https://www.youtube.com/watch?v=MJ3bvCkHJtE">Perlin Noise Explained Tutorial 2</see> by <see href="https://www.youtube.com/@Fataho">Fataho</see></description></item>
/// <item><description>Wikipedia, <see href="https://en.wikipedia.org/wiki/Perlin_noise">Perlin noise</see></description></item>
/// <item><description>TODO: Add explanation for seeding</description></item>
/// </list>
/// </summary>

public class Perlin3D
{
    /// <summary>
    /// Recomended gridvectors for perlin noise represented in bitformat 00=0, 01=1, 10=-1.
    /// <br/>Retrieved from article <see href="https://weber.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf">Simplex noise demystified</see>
    /// </summary>
    static int[] possibleVectors =
    {
        0b00_01_01,
        0b00_10_01,
        0b00_01_10,
        0b00_10_10,
        0b01_00_01,
        0b10_00_01,
        0b01_00_10,
        0b10_00_10,
        0b01_01_00,
        0b10_01_00,
        0b01_10_00,
        0b10_10_00
    };

    //Permarray is used to give pseudo random vectors in corners of grid,
    //Contains an equal distribution of the possible vectors. 
    //generated using the seed, one uniqe permarray for each seed. Larger permarray results in less repeating.
    int permLength = 32;
    int[] perm;

    /// <summary>
    /// Constructor for Perlin3D 
    /// </summary>
    /// <param name="seed">Each seed generates a unique perlin noise</param>
    public Perlin3D(ulong seed)
    {
        InitPermArray(seed);
    }

    /// <summary>
    /// Initialize the perm array based on given <paramref name="seed"/>.
    /// <br/>Each seed will generate a uniqe array
    /// </summary>
    /// <param name="seed">Seed for seeding the array</param>
    void InitPermArray(ulong seed)
    {
        perm = new int[permLength + 1]; //+1 so that we can loop and get same values on edges

        int seed0 = (int)(seed % 479001600); //Seed % 12!
        seed /= 479001600;                  //Shift down with 12!
        int seed1 = (int)(seed % 479001600); //Seed % 12!
        seed /= 479001600;                  //Shift down with 12!
        int seed2 = (int)(seed % 479001600); //Seed % 12!

        //Get the perm arrays for the three different sections of the perm array using the seed as index
        int[] permList0 = GetPerm(possibleVectors.Length - 1, seed0).ToArray();
        int[] permList1 = GetPerm(possibleVectors.Length - 1, seed1).ToArray();
        int[] permList2 = GetPerm(possibleVectors.Length - 1, seed2).ToArray();

        //Initialize first part of perm array
        for (int i = 0; i < 12; i++)
        {
            int index = permList0[i];
            perm[i] = possibleVectors[index];
        }

        //Initialize second part of perm array
        for (int i = 12; i < 24; i++)
        {
            perm[i] = possibleVectors[permList1[i % 12]];
        }

        //Initialize last part of perm array
        for (int i = 24; i < 32; i++)
        {
            perm[i] = possibleVectors[permList2[i % 12]];
        }

        //Set last value to first to create a cyclic array
        perm[permLength] = perm[0];
    }

    /// <summary>
    /// Each <paramref name="index"/> corresponds to one unique permutation of the number from 0 to <paramref name="amount"/>,
    /// <br/>see documentation for how the algoritm works 
    /// </summary>
    /// <param name="amount">Upper bound of numbers to permute</param>
    /// <param name="index">Index to choose permutation</param>
    /// <returns>Returns the given permutation</returns>
    List<int> GetPerm(int amount, int index)
    {
        return GetPerm(0, amount, index);
    }


    /// <summary>
    /// Each <paramref name="index"/> corresponds to one unique permutation of the number from <paramref name="lower"/> to <paramref name="upper"/>,
    /// <br/>see documentation for how the algoritm works 
    /// </summary>
    /// <param name="lower">Lower bound of numbers to permute</param>
    /// <param name="upper">Upper bound of numbers to permute</param>
    /// <param name="index">Index to choose permutation</param>
    /// <returns>Returns the given permutation</returns>
    List<int> GetPerm(int lower, int upper, int index)
    {
        int range = upper - lower;
        if (range == 0)
        {
            return new List<int>() { upper };
        }
        int length = Factorial(range + 1);
        index = index % length;
        int lowersRow = Mathf.FloorToInt((((float)index) / ((float)length)) * ((float)range + 1));

        List<int> subPerm = GetPerm(lower + 1, upper, index);
        subPerm.Insert(lowersRow, lower);
        return subPerm;
    }

    /// <summary>
    /// Calculates the factorial of fact
    /// </summary>
    /// <param name="fact">Number to calculate factorial with</param>
    /// <returns>Returns the factorial</returns>
    int Factorial(int fact)
    {
        int res = 1;
        for (int i = 1; i < fact + 1; i++)
        {
            res *= i;
        }
        return res;
    }

    /// <summary>
    /// Calculates the noise value for given position (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>).
    /// <br/> Uses the perlin noise method, for more information see article, <see href="https://weber.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf">Simplex noise demystified</see>
    /// </summary>
    /// <param name="x">x-coord.</param>
    /// <param name="y">y-coord.</param>
    /// <param name="z">z-coord.</param>
    /// <returns>float between (-1, 1)</returns>
    public float Noise(float x, float y, float z)
    {
        //Get (X, Y, Z) for the cube we are in
        int X = Mathf.FloorToInt(x) & (permLength - 1);
        int Y = Mathf.FloorToInt(y) & (permLength - 1);
        int Z = Mathf.FloorToInt(z) & (permLength - 1);

        //Get our position relative to the cube we are in
        x -= Mathf.FloorToInt(x);
        y -= Mathf.FloorToInt(y);
        z -= Mathf.FloorToInt(z);

        //Calculate the fade function
        float u = Fade(x);
        float v = Fade(y);
        float w = Fade(z);

        //Get the gradient vectors, important that shared corners in adjacent cubes have same gradient vectors,
        //recal perm[perm.length] = perm[0] (loops around)
        int A = (perm[X] + Y) & (permLength - 1);
        int B = (perm[X + 1] + Y) & (permLength - 1);
        int AA = (perm[A] + Z) & (permLength - 1);
        int AB = (perm[B] + Z) & (permLength - 1);
        int BA = (perm[A + 1] + Z) & (permLength - 1);
        int BB = (perm[B + 1] + Z) & (permLength - 1);

        //Please read article to understand belowe code
        return Interpolate(w, Interpolate(v, Interpolate(u, DotProduct(perm[AA], x, y, z), DotProduct(perm[AB], x - 1, y, z)), Interpolate(u, DotProduct(perm[BA], x, y - 1, z), DotProduct(perm[BB], x - 1, y - 1, z))), Interpolate(v, Interpolate(u, DotProduct(perm[AA + 1], x, y, z - 1), DotProduct(perm[AB + 1], x - 1, y, z - 1)), Interpolate(u, DotProduct(perm[BA + 1], x, y - 1, z - 1), DotProduct(perm[BB + 1], x - 1, y - 1, z - 1))));
    }

    /// <summary>
    /// Calculates the noise value for given position vector <paramref name="pos"/>.
    /// <br/> Uses the perlin noise method, for more information see article,
    /// <see href="https://weber.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf">Simplex noise demystified</see>
    /// </summary>
    /// <param name="pos">Position to calulate noise at</param>
    /// <returns>noise value for given position <paramref name="pos"/></returns>
    public float Noise(Vector3 pos)
    {
        return Noise(pos.x, pos.y, pos.z);
    }

    /// <summary>
    /// The interpolation function makes a linear interpolation using the fade value <paramref name="t"/>, 
    /// this is used to smooth transitions between point <paramref name="a"/> and point <paramref name="b"/>
    /// <br/> See following video for explanation of interpolation, <see href="https://www.youtube.com/watch?v=ip0XBBBY6A8">Interpolation</see> 
    /// by <see href="https://www.youtube.com/@mathsgenie7808">Maths Genie</see>
    /// </summary>
    /// <param name="t">Fade value</param>
    /// <param name="a">Point a</param>
    /// <param name="b">Point b</param>
    /// <returns>Returns the smoothed of value from point <paramref name="a"/> to <paramref name="b"/></returns>
    static float Interpolate(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    /// <summary>
    /// Fade function used to create a smoother transition between noise points, 
    /// <br/>this function is the recomended perlin noise fade function, 
    /// <br/>see article <see href="https://weber.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf">Simplex noise demystified</see> for more information
    /// </summary>
    /// <param name="t">Value to fade</param>
    /// <returns>Returns a fade value for use in <see cref="Interpolation"/></returns>
    float Fade(float t)
    {
        return t * t * t * (t * (t * 6 - 15) + 10);
    }

    /// <summary>
    /// Calculates the dot product between <paramref name="vector"/> and vector (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>)
    /// </summary>
    /// <param name="vector">Vector on form 0bXX_YY_ZZ, where XX, YY and ZZ can be of value 00=0, 01=1, 10=-1</param>
    /// <param name="x">x-coord.</param>
    /// <param name="y">y-coord.</param>
    /// <param name="z">z-coord.</param>
    /// <returns>dot product</returns>
    float DotProduct(int vector, float x, float y, float z)
    {
        x *= (vector & 0b11_00_00) == 0 ? 0 : (vector & 0b11_00_00) == 0b01_00_00 ? 1 : -1;
        y *= (vector & 0b00_11_00) == 0 ? 0 : (vector & 0b00_11_00) == 0b00_01_00 ? 1 : -1;
        z *= (vector & 0b00_00_11) == 0 ? 0 : (vector & 0b00_00_11) == 0b00_00_01 ? 1 : -1;
        return x + y + z;
    }
}