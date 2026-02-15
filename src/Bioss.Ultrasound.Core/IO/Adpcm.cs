namespace Bioss.Ultrasound.Core.IO;

//https://github.com/eurotools/es-ima-adpcm-encoder-decoder
public static class Adpcm
{
    public class ImaAdpcmState
    {
        public int ValPrev;
        public int Index;
    }

    // Intel ADPCM step variation table
    private static readonly int[] indexTable = {
        -1, -1, -1, -1, 2, 4, 6, 8,
        -1, -1, -1, -1, 2, 4, 6, 8,
    };

    private static readonly int[] stepsizeTable = {
        7, 8, 9, 10, 11, 12, 13, 14, 16, 17,
        19, 21, 23, 25, 28, 31, 34, 37, 41, 45,
        50, 55, 60, 66, 73, 80, 88, 97, 107, 118,
        130, 143, 157, 173, 190, 209, 230, 253, 279, 307,
        337, 371, 408, 449, 494, 544, 598, 658, 724, 796,
        876, 963, 1060, 1166, 1282, 1411, 1552, 1707, 1878, 2066,
        2272, 2499, 2749, 3024, 3327, 3660, 4026, 4428, 4871, 5358,
        5894, 6484, 7132, 7845, 8630, 9493, 10442, 11487, 12635, 13899,
        15289, 16818, 18500, 20350, 22385, 24623, 27086, 29794, 32767
    };

    public static short[] Decode(byte[] ImaFileData, int numSamples, ImaAdpcmState inState = null)
    {
        int inp;            /* Input buffer pointer */
        short[] outBuff = new short[numSamples]; /* output buffer pointer */
        int outIndex = 0;   /* Output buffer pointer */
        int sign;           /* Current adpcm sign bit */
        int delta;          /* Current adpcm output value */
        int step;           /* Stepsize */
        int valpred;        /* Predicted value */
        int vpdiff;         /* Current change to valpred */
        int index;          /* Current step change index */
        int inputbuffer;    /* Place to keep next 4-bit value */
        bool bufferstep;    /* Toggle between inputbuffer/input */

        ImaAdpcmState state = inState ?? new ImaAdpcmState();
        inp = 0;
        inputbuffer = 0;

        valpred = state.ValPrev;
        index = state.Index;
        step = stepsizeTable[index];

        bufferstep = false;

        for (; numSamples > 0; numSamples--)
        {
            /* Step 1 - get the delta value */
            if (bufferstep)
            {
                delta = inputbuffer & 0xf;
            }
            else
            {
                inputbuffer = ImaFileData[inp++];
                delta = inputbuffer >> 4 & 0xf;
            }
            bufferstep = !bufferstep;

            /* Step 2 - Find new index value (for later) */
            index += indexTable[delta];
            if (index < 0) index = 0;
            if (index > 88) index = 88;

            /* Step 3 - Separate sign and magnitude */
            sign = delta & 8;
            delta &= 7;

            /* Step 4 - Compute difference and new predicted value */
            /*
            ** Computes 'vpdiff = (delta+0.5)*step/4', but see comment
            ** in adpcm_coder.
            */
            vpdiff = step >> 3;
            if ((delta & 4) != 0) vpdiff += step;
            if ((delta & 2) != 0) vpdiff += step >> 1;
            if ((delta & 1) != 0) vpdiff += step >> 2;

            if (sign != 0)
                valpred -= vpdiff;
            else
                valpred += vpdiff;

            /* Step 5 - clamp output value */
            if (valpred > short.MaxValue)
                valpred = short.MaxValue;
            else if (valpred < short.MinValue)
                valpred = short.MinValue;

            /* Step 6 - Update step value */
            step = stepsizeTable[index];

            /* Step 7 - Output value */
            outBuff[outIndex++] = (short)valpred;
        }

        state.ValPrev = valpred;
        state.Index = index;

        return outBuff;
    }

    public static byte[] Encode(short[] input)
    {
        MemoryStream outBuff = new MemoryStream();
        int inp;			    /* Input buffer pointer */
        int val;                /* Current input sample value */
        int sign;               /* Current adpcm sign bit */
        int delta;              /* Current adpcm output value */
        int diff;               /* Difference between val and valprev */
        int step;               /* Stepsize */
        int valpred;            /* Predicted output value */
        int vpdiff;             /* Current change to valpred */
        int index;              /* Current step change index */
        int outputbuffer;       /* place to keep previous 4-bit value */
        bool bufferstep;        /* toggle between outputbuffer/output */
        int numSamples;         /* Number of Samples to encode*/
        ImaAdpcmState state = new ImaAdpcmState();

        outputbuffer = 0;
        inp = 0;
        numSamples = input.Length;

        valpred = state.ValPrev;
        index = state.Index;
        step = stepsizeTable[index];

        bufferstep = true;

        for (; numSamples > 0; numSamples--)
        {
            val = input[inp++];

            /* Step 1 - compute difference with previous value */
            diff = val - valpred;
            sign = diff < 0 ? 8 : 0;
            if (sign != 0) diff = -diff;

            /* Step 2 - Divide and clamp */
            /* Note:
            ** This code *approximately* computes:
            **    delta = diff*4/step;
            **    vpdiff = (delta+0.5)*step/4;
            ** but in shift step bits are dropped. The net result of this is
            ** that even if you have fast mul/div hardware you cannot put it to
            ** good use since the fixup would be too expensive.
            */
            delta = 0;
            vpdiff = step >> 3;

            if (diff >= step)
            {
                delta = 4;
                diff -= step;
                vpdiff += step;
            }
            step >>= 1;
            if (diff >= step)
            {
                delta |= 2;
                diff -= step;
                vpdiff += step;
            }
            step >>= 1;
            if (diff >= step)
            {
                delta |= 1;
                vpdiff += step;
            }

            /* Step 3 - Update previous value */
            if (sign != 0)
                valpred -= vpdiff;
            else
                valpred += vpdiff;

            /* Step 4 - Clamp previous value to 16 bits */
            if (valpred > short.MaxValue)
                valpred = short.MaxValue;
            else if (valpred < short.MinValue)
                valpred = short.MinValue;

            /* Step 5 - Assemble value, update index and step values */
            delta |= sign;

            index += indexTable[delta];
            if (index < 0) index = 0;
            if (index > 88) index = 88;
            step = stepsizeTable[index];

            /* Step 6 - Output value */
            if (bufferstep)
            {
                outputbuffer = delta << 4 & 0xf0;
            }
            else
            {
                outBuff.WriteByte((byte)(delta & 0x0f | outputbuffer));
            }
            bufferstep = !bufferstep;
        }

        /* Output last step, if needed */
        if (!bufferstep)
        {
            outBuff.WriteByte((byte)outputbuffer);
        }

        state.ValPrev = valpred;
        state.Index = index;

        return outBuff.ToArray();
    }
}

