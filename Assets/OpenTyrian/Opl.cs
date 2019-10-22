using static System.Math;
//using static LoudnessC;


public static class OplC
{
    /*
 *  Copyright (C) 2002-2010  The DOSBox Team
 *  OPL2/OPL3 emulation library
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 * 
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 * 
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301  USA
 */


    /*
     * Originally based on ADLIBEMU.C, an AdLib/OPL2 emulation library by Ken Silverman
     * Copyright (C) 1998-2001 Ken Silverman
     * Ken Silverman's official web site: "http://www.advsys.net/ken"
     */




    /*
        define attribution that inlines/forces inlining of a function (optional)
    */

#if OPLTYPE_IS_OPL3
public const int NUM_CHANNELS =	18;
#else
    public const int NUM_CHANNELS = 9;
#endif

    public const int MAXOPERATORS = (NUM_CHANNELS * 2);

    public const double PI = 3.1415926535897932384626433832795;

    public const int FIXEDPT = 0x10000;     // fixed-point calculations using 16+16
    public const int FIXEDPT_LFO = 0x1000000;   // fixed-point calculations using 8+24

    public const int WAVEPREC = 1024;       // waveform precision (10 bits)

    public const double INTFREQU = 14318180.0 / 288.0;		// clocking of the chip

    public const int OF_TYPE_ATT = 0;
    public const int OF_TYPE_DEC = 1;
    public const int OF_TYPE_REL = 2;
    public const int OF_TYPE_SUS = 3;
    public const int OF_TYPE_SUS_NOKEEP = 4;
    public const int OF_TYPE_OFF = 5;

    public const int ARC_CONTROL = 0x00;
    public const int ARC_TVS_KSR_MUL = 0x20;
    public const int ARC_KSL_OUTLEV = 0x40;
    public const int ARC_ATTR_DECR = 0x60;
    public const int ARC_SUSL_RELR = 0x80;
    public const int ARC_FREQ_NUM = 0xa0;
    public const int ARC_KON_BNUM = 0xb0;
    public const int ARC_PERC_MODE = 0xbd;
    public const int ARC_FEEDBACK = 0xc0;
    public const int ARC_WAVE_SEL = 0xe0;

    public const int ARC_SECONDSET = 0x100;     // second operator set for OPL3


    public const int OP_ACT_OFF = 0x00;
    public const int OP_ACT_NORMAL = 0x01;      // regular channel activated (bitmasked)
    public const int OP_ACT_PERC = 0x02;       // percussion channel activated (bitmasked)

    public const int BLOCKBUF_SIZE = 512;


    // vibrato constants
    public const int VIBTAB_SIZE = 8;
    public const double VIBFAC = 70 / 50000;     // no braces, integer mul/div

    // tremolo constants and table
    public const int TREMTAB_SIZE = 53;
    public const double TREM_FREQ = 3.7;            // tremolo at 3.7hz


    /* operator struct definition
         For OPL2 all 9 channels consist of two operators each, carrier and modulator.
         Channel x has operators x as modulator and operators (9+x) as carrier.
         For OPL3 all 18 channels consist either of two operators (2op mode) or four
         operators (4op mode) which is determined through register4 of the second
         adlib register set.
         Only the channels 0,1,2 (first set) and 9,10,11 (second set) can act as
         4op channels. The two additional operators for a channel y come from the
         2op channel y+3 so the operatorss y, (9+y), y+3, (9+y)+3 make up a 4op
         channel.
    */
    struct op_type
    {
        public int cval, lastcval;          // current output/last output (used for feedback)
        public uint tcount, wfpos, tinc;     // time (position in waveform) and time increment
        public double amp, step_amp;           // and amplification (envelope)
        public double vol;                     // volume
        public double sustain_level;           // sustain level
        public int mfbi;                    // feedback amount
        public double a0, a1, a2, a3;          // attack rate function coefficients
        public double decaymul, releasemul;    // decay/release rate functions
        public uint op_state;                // current state of operator (attack/decay/sustain/release/off)
        public uint toff;
        public int freq_high;               // highest three bits of the frequency, used for vibrato calculations
        public uint cur_wformIdx;              // start of selected waveform; use as wavtable[cur_wform + (desired offset)]
        public uint cur_wmask;               // mask for selected waveform
        public uint act_state;               // activity state (regular, percussion)
        public bool sus_keep;                  // keep sustain level when decay finished
        public bool vibrato, tremolo;          // vibrato/tremolo enable bits

        // variables used to provide non-continuous envelopes
        public uint generator_pos;           // for non-standard sample rates we need to determine how many samples have passed
        public int cur_env_step;              // current (standardized) sample position
        public int env_step_a, env_step_d, env_step_r;    // number of std samples of one step (for attack/decay/release mode)
        public byte step_skip_pos_a;          // position of 8-cyclic step skipping (always 2^x to check against mask)
        public int env_step_skip_a;           // bitmask that determines if a step is skipped (respective bit is zero then)

#if OPLTYPE_IS_OPL3
	bool is_4op,is_4op_attached;	// base of a 4op channel/part of a 4op channel
	int left_pan,right_pan;		// opl3 stereo panning amount
#endif
    }

    // per-chip variables
    static uint chip_num;
    static op_type[] op = new op_type[MAXOPERATORS];

    static int int_samplerate;

    static byte status;
    static uint opl_index;
#if OPLTYPE_IS_OPL3
byte adlibreg[512];	// adlib register set (including second set)
byte wave_sel[44];		// waveform selection
#else
    static byte[] adlibreg = new byte[256];    // adlib register set
    static byte[] wave_sel = new byte[22];     // waveform selection
#endif


    // vibrato/tremolo increment/counter
    static uint vibtab_pos;
    static uint vibtab_add;
    static uint tremtab_pos;
    static uint tremtab_add;


    public static void opl_init(uint freq)
    {
        adlib_init(freq);
    }
    public static void opl_write(int reg, byte val)
    {
        adlib_write((uint)reg, val);
    }
    public static void opl_update(float[] sndptr, int sndptrIdx, int numsamples)
    {
        adlib_getsample(sndptr, sndptrIdx, numsamples);
    }


    static uint generator_add;    // should be a chip parameter

    static double recipsamp;    // inverse of sampling rate
    static short[] wavtable = new short[WAVEPREC * 3];   // wave form table

    // vibrato/tremolo tables
    static int[] vib_table = new int[VIBTAB_SIZE];
    static int[] trem_table = new int[TREMTAB_SIZE * 2];

    static int[] vibval_const = new int[BLOCKBUF_SIZE];
    static int[] tremval_const = new int[BLOCKBUF_SIZE];

    // vibrato value tables (used per-operator)
    static int[] vibval_var1 = new int[BLOCKBUF_SIZE];
    static int[] vibval_var2 = new int[BLOCKBUF_SIZE];
    //static int vibval_var3[BLOCKBUF_SIZE];
    //static int vibval_var4[BLOCKBUF_SIZE];

    // vibrato/trmolo value table pointers
    //ED TODO: Should these be indices instead?
    static int[] vibval1, vibval2, vibval3, vibval4;
    static int[] tremval1, tremval2, tremval3, tremval4;


    // key scale level lookup table
    static readonly double[] kslmul = {
    0.0, 0.5, 0.25, 1.0		// . 0, 3, 1.5, 6 dB/oct
};

    // frequency multiplicator lookup table
    static readonly double[] frqmul_tab = {
    0.5,1,2,3,4,5,6,7,8,9,10,10,12,12,15,15
    };
    // calculated frequency multiplication values (depend on sampling rate)
    static double[] frqmul = new double[16];

    // key scale levels
    static byte[][] kslev = VarzC.DoubleEmptyArray<byte>(8, 16, 0);

    // map a channel number to the register offset of the modulator (=register base)
    static readonly byte[] modulatorbase = {
    0,1,2,
    8,9,10,
    16,17,18
};

    // map a register base to a modulator operator number or operator number
#if OPLTYPE_IS_OPL3
static const byte regbase2modop[44] = {
	0,1,2,0,1,2,0,0,3,4,5,3,4,5,0,0,6,7,8,6,7,8,					// first set
	18,19,20,18,19,20,0,0,21,22,23,21,22,23,0,0,24,25,26,24,25,26	// second set
};
static const byte regbase2op[44] = {
	0,1,2,9,10,11,0,0,3,4,5,12,13,14,0,0,6,7,8,15,16,17,			// first set
	18,19,20,27,28,29,0,0,21,22,23,30,31,32,0,0,24,25,26,33,34,35	// second set
};
#else
    static readonly byte[] regbase2modop = {
    0,1,2,0,1,2,0,0,3,4,5,3,4,5,0,0,6,7,8,6,7,8
};
    static readonly byte[] regbase2op = {
    0,1,2,9,10,11,0,0,3,4,5,12,13,14,0,0,6,7,8,15,16,17
};
#endif


    // start of the waveform
    static uint[] waveform = {
    WAVEPREC,
    WAVEPREC>>1,
    WAVEPREC,
    (WAVEPREC*3)>>2,
    0,
    0,
    (WAVEPREC*5)>>2,
    WAVEPREC<<1
};

    // length of the waveform as mask
    static uint[] wavemask = {
    WAVEPREC-1,
    WAVEPREC-1,
    (WAVEPREC>>1)-1,
    (WAVEPREC>>1)-1,
    WAVEPREC-1,
    ((WAVEPREC*3)>>2)-1,
    WAVEPREC>>1,
    WAVEPREC-1
    };

    // where the first entry resides
    static uint[] wavestart = {
    0,
    WAVEPREC>>1,
    0,
    WAVEPREC>>2,
    0,
    0,
    0,
    WAVEPREC>>3
    };

    // envelope generator function constants
    static double[] attackconst = {
    1/2.82624,
    1/2.25280,
    1/1.88416,
    1/1.59744
};
    static double[] decrelconst = {
    1/39.28064,
    1/31.41608,
    1/26.17344,
    1/22.44608
};


    static void operator_advance(ref op_type op_pt, int vib)
    {
        op_pt.wfpos = op_pt.tcount;                       // waveform position

        // advance waveform time
        op_pt.tcount += op_pt.tinc;
        op_pt.tcount = (uint)(op_pt.tcount + (int)(op_pt.tinc) * vib / FIXEDPT);

        op_pt.generator_pos += generator_add;
    }

    static void operator_advance_drums(ref op_type op_pt1, int vib1, ref op_type op_pt2, int vib2, ref op_type op_pt3, int vib3)
    {
        uint c1 = op_pt1.tcount / FIXEDPT;
        uint c3 = op_pt3.tcount / FIXEDPT;
        int phasebit = ((((c1 & 0x88) ^ ((c1 << 5) & 0x80)) | ((c3 ^ (c3 << 2)) & 0x20)) != 0) ? 0x02 : 0x00;

        uint noisebit = LibC.mt_rand() & 1;

        uint snare_phase_bit = (((op_pt1.tcount / FIXEDPT) / 0x100)) & 1;

        //Hihat
        uint inttm = (uint)((phasebit << 8) | (0x34 << (int)(phasebit ^ (noisebit << 1))));
        op_pt1.wfpos = inttm * FIXEDPT;                // waveform position
                                                       // advance waveform time
        op_pt1.tcount += op_pt1.tinc;
        op_pt1.tcount = (uint)(op_pt1.tcount + (int)(op_pt1.tinc) * vib1 / FIXEDPT);
        op_pt1.generator_pos += generator_add;

        //Snare
        inttm = ((1 + snare_phase_bit) ^ noisebit) << 8;
        op_pt2.wfpos = inttm * FIXEDPT;                // waveform position
                                                       // advance waveform time
        op_pt2.tcount += op_pt2.tinc;
        op_pt2.tcount = (uint)(op_pt2.tcount + (int)(op_pt2.tinc) * vib2 / FIXEDPT);
        op_pt2.generator_pos += generator_add;

        //Cymbal
        inttm = (uint)((1 + phasebit) << 8);
        op_pt3.wfpos = inttm * FIXEDPT;                // waveform position
                                                       // advance waveform time
        op_pt3.tcount += op_pt3.tinc;
        op_pt3.tcount = (uint)(op_pt3.tcount + (int)(op_pt3.tinc) * vib3 / FIXEDPT);
        op_pt3.generator_pos += generator_add;
    }


    // output level is sustained, mode changes only when operator is turned off (.release)
    // or when the keep-sustained bit is turned off (.sustain_nokeep)
    static void operator_output(ref op_type op_pt, int modulator, int trem)
    {
        if (op_pt.op_state != OF_TYPE_OFF)
        {
            op_pt.lastcval = op_pt.cval;
            uint i = (uint)((op_pt.wfpos + modulator) / FIXEDPT);

            // wform: -16384 to 16383 (0x4000)
            // trem :  32768 to 65535 (0x10000)
            // step_amp: 0.0 to 1.0
            // vol  : 1/2^14 to 1/2^29 (/0x4000; /1../0x8000)

            op_pt.cval = (int)(op_pt.step_amp * op_pt.vol * wavtable[op_pt.cur_wformIdx + (i & op_pt.cur_wmask)] * trem / 16.0);
        }
    }


    // no action, operator is off
    static void operator_off(ref op_type op_pt)
    {
        //(void)op_pt;
    }

    // output level is sustained, mode changes only when operator is turned off (.release)
    // or when the keep-sustained bit is turned off (.sustain_nokeep)
    static void operator_sustain(ref op_type op_pt)
    {
        uint num_steps_add = op_pt.generator_pos / FIXEDPT;  // number of (standardized) samples
        for (uint ct = 0; ct < num_steps_add; ct++)
        {
            op_pt.cur_env_step++;
        }
        op_pt.generator_pos -= num_steps_add * FIXEDPT;
    }

    // operator in release mode, if output level reaches zero the operator is turned off
    static void operator_release(ref op_type op_pt)
    {
        // ??? boundary?
        if (op_pt.amp > 0.00000001)
        {
            // release phase
            op_pt.amp *= op_pt.releasemul;
        }

        uint num_steps_add = op_pt.generator_pos / FIXEDPT;  // number of (standardized) samples
        for (uint ct = 0; ct < num_steps_add; ct++)
        {
            op_pt.cur_env_step++;                      // sample counter
            if ((op_pt.cur_env_step & op_pt.env_step_r) == 0)
            {
                if (op_pt.amp <= 0.00000001)
                {
                    // release phase finished, turn off this operator
                    op_pt.amp = 0.0;
                    if (op_pt.op_state == OF_TYPE_REL)
                    {
                        op_pt.op_state = OF_TYPE_OFF;
                    }
                }
                op_pt.step_amp = op_pt.amp;
            }
        }
        op_pt.generator_pos -= num_steps_add * FIXEDPT;
    }

    // operator in decay mode, if sustain level is reached the output level is either
    // kept (sustain level keep enabled) or the operator is switched into release mode
    static void operator_decay(ref op_type op_pt)
    {
        if (op_pt.amp > op_pt.sustain_level)
        {
            // decay phase
            op_pt.amp *= op_pt.decaymul;
        }

        uint num_steps_add = op_pt.generator_pos / FIXEDPT;  // number of (standardized) samples
        for (uint ct = 0; ct < num_steps_add; ct++)
        {
            op_pt.cur_env_step++;
            if ((op_pt.cur_env_step & op_pt.env_step_d) == 0)
            {
                if (op_pt.amp <= op_pt.sustain_level)
                {
                    // decay phase finished, sustain level reached
                    if (op_pt.sus_keep)
                    {
                        // keep sustain level (until turned off)
                        op_pt.op_state = OF_TYPE_SUS;
                        op_pt.amp = op_pt.sustain_level;
                    }
                    else
                    {
                        // next: release phase
                        op_pt.op_state = OF_TYPE_SUS_NOKEEP;
                    }
                }
                op_pt.step_amp = op_pt.amp;
            }
        }
        op_pt.generator_pos -= num_steps_add * FIXEDPT;
    }

    // operator in attack mode, if full output level is reached,
    // the operator is switched into decay mode
    static void operator_attack(ref op_type op_pt)
    {
        op_pt.amp = ((op_pt.a3 * op_pt.amp + op_pt.a2) * op_pt.amp + op_pt.a1) * op_pt.amp + op_pt.a0;

        uint num_steps_add = op_pt.generator_pos / FIXEDPT;      // number of (standardized) samples
        for (uint ct = 0; ct < num_steps_add; ct++)
        {
            op_pt.cur_env_step++;  // next sample
            if ((op_pt.cur_env_step & op_pt.env_step_a) == 0)
            {       // check if next step already reached
                if (op_pt.amp > 1.0)
                {
                    // attack phase finished, next: decay
                    op_pt.op_state = OF_TYPE_DEC;
                    op_pt.amp = 1.0;
                    op_pt.step_amp = 1.0;
                }
                op_pt.step_skip_pos_a <<= 1;
                if (op_pt.step_skip_pos_a == 0) op_pt.step_skip_pos_a = 1;
                if ((op_pt.step_skip_pos_a & op_pt.env_step_skip_a) != 0)
                {   // check if required to skip next step
                    op_pt.step_amp = op_pt.amp;
                }
            }
        }
        op_pt.generator_pos -= num_steps_add * FIXEDPT;
    }


    delegate void optype_fptr(ref op_type op_type);

    static optype_fptr[] opfuncs = {
        operator_attack,
        operator_decay,
        operator_release,
        operator_sustain,	// sustain phase (keeping level)
	    operator_release,	// sustain_nokeep phase (release-style)
	    operator_off
    };

    static void change_attackrate(uint regbase, ref op_type op_pt)
    {
        int attackrate = adlibreg[ARC_ATTR_DECR + regbase] >> 4;
        if (attackrate != 0)
        {
            double f = Pow(2.0, (double)attackrate + (op_pt.toff >> 2) - 1) * attackconst[op_pt.toff & 3] * recipsamp;
            // attack rate coefficients
            op_pt.a0 = 0.0377 * f;
            op_pt.a1 = 10.73 * f + 1;
            op_pt.a2 = -17.57 * f;
            op_pt.a3 = 7.42 * f;

            int step_skip = attackrate * 4 + (int)op_pt.toff;
            int steps = step_skip >> 2;
            op_pt.env_step_a = (1 << (steps <= 12 ? 12 - steps : 0)) - 1;

            int step_num = (step_skip <= 48) ? (4 - (step_skip & 3)) : 0;
            byte[] step_skip_mask = { 0xff, 0xfe, 0xee, 0xba, 0xaa };
            op_pt.env_step_skip_a = step_skip_mask[step_num];

#if OPLTYPE_IS_OPL3
		if (step_skip>=60) {
#else
            if (step_skip >= 62)
            {
#endif
                op_pt.a0 = 2.0;  // something that triggers an immediate transition to amp:=1.0
                op_pt.a1 = 0.0;
                op_pt.a2 = 0.0;
                op_pt.a3 = 0.0;
            }
        }
        else
        {
            // attack disabled
            op_pt.a0 = 0.0;
            op_pt.a1 = 1.0;
            op_pt.a2 = 0.0;
            op_pt.a3 = 0.0;
            op_pt.env_step_a = 0;
            op_pt.env_step_skip_a = 0;
        }
    }

    static void change_decayrate(uint regbase, ref op_type op_pt)
    {
        int decayrate = adlibreg[ARC_ATTR_DECR + regbase] & 15;
        // decaymul should be 1.0 when decayrate==0
        if (decayrate != 0)
        {
            double f = -7.4493 * decrelconst[op_pt.toff & 3] * recipsamp;
            op_pt.decaymul = Pow(2.0, f * Pow(2.0, decayrate + (op_pt.toff >> 2)));
            int steps = (decayrate * 4 + (int)op_pt.toff) >> 2;
            op_pt.env_step_d = (1 << (steps <= 12 ? 12 - steps : 0)) - 1;
        }
        else
        {
            op_pt.decaymul = 1.0;
            op_pt.env_step_d = 0;
        }
    }

    static void change_releaserate(uint regbase, ref op_type op_pt)
    {
        int releaserate = adlibreg[ARC_SUSL_RELR + regbase] & 15;
        // releasemul should be 1.0 when releaserate==0
        if (releaserate != 0)
        {
            double f = -7.4493 * decrelconst[op_pt.toff & 3] * recipsamp;
            op_pt.releasemul = Pow(2.0, f * Pow(2.0, releaserate + (op_pt.toff >> 2)));
            int steps = (releaserate * 4 + (int)op_pt.toff) >> 2;
            op_pt.env_step_r = (1 << (steps <= 12 ? 12 - steps : 0)) - 1;
        }
        else
        {
            op_pt.releasemul = 1.0;
            op_pt.env_step_r = 0;
        }
    }

    static void change_sustainlevel(uint regbase, ref op_type op_pt)
    {
        int sustainlevel = adlibreg[ARC_SUSL_RELR + regbase] >> 4;
        // sustainlevel should be 0.0 when sustainlevel==15 (max)
        if (sustainlevel < 15)
        {
            op_pt.sustain_level = Pow(2.0, sustainlevel * (-0.5));
        }
        else
        {
            op_pt.sustain_level = 0.0;
        }
    }

    static void change_waveform(uint regbase, ref op_type op_pt)
    {
#if OPLTYPE_IS_OPL3
	if (regbase>=ARC_SECONDSET) regbase -= (ARC_SECONDSET-22);	// second set starts at 22
#endif
        // waveform selection
        op_pt.cur_wmask = wavemask[wave_sel[regbase]];
        op_pt.cur_wformIdx = waveform[wave_sel[regbase]];
        // (might need to be adapted to waveform type here...)
    }

    static void change_keepsustain(uint regbase, ref op_type op_pt)
    {
        op_pt.sus_keep = (adlibreg[ARC_TVS_KSR_MUL + regbase] & 0x20) > 0;
        if (op_pt.op_state == OF_TYPE_SUS)
        {
            if (!op_pt.sus_keep) op_pt.op_state = OF_TYPE_SUS_NOKEEP;
        }
        else if (op_pt.op_state == OF_TYPE_SUS_NOKEEP)
        {
            if (op_pt.sus_keep) op_pt.op_state = OF_TYPE_SUS;
        }
    }

    // enable/disable vibrato/tremolo LFO effects
    static void change_vibrato(uint regbase, ref op_type op_pt)
    {
        op_pt.vibrato = (adlibreg[ARC_TVS_KSR_MUL + regbase] & 0x40) != 0;
        op_pt.tremolo = (adlibreg[ARC_TVS_KSR_MUL + regbase] & 0x80) != 0;
    }

    // change amount of self-feedback
    static void change_feedback(uint chanbase, ref op_type op_pt)
    {
        int feedback = adlibreg[ARC_FEEDBACK + chanbase] & 14;
        if (feedback != 0) op_pt.mfbi = (int)(Pow(2.0, (feedback >> 1) + 8));
        else op_pt.mfbi = 0;
    }

    static void change_frequency(uint chanbase, uint regbase, ref op_type op_pt)
    {
        // frequency
        uint frn = ((((uint)adlibreg[ARC_KON_BNUM + chanbase]) & 3) << 8) + adlibreg[ARC_FREQ_NUM + chanbase];

        // block number/octave
        uint oct = ((((uint)adlibreg[ARC_KON_BNUM + chanbase]) >> 2) & 7);
        op_pt.freq_high = (int)((frn >> 7) & 7);

        // keysplit
        uint note_sel = (uint)(adlibreg[8] >> 6) & 1;
        op_pt.toff = ((frn >> 9) & (note_sel ^ 1)) | ((frn >> 8) & note_sel);
        op_pt.toff += (oct << 1);

        // envelope scaling (KSR)
        if (!((adlibreg[ARC_TVS_KSR_MUL + regbase] & 0x10) != 0)) op_pt.toff >>= 2;

        // 20+a0+b0:
        op_pt.tinc = (uint)(((frn << (int)oct) * frqmul[adlibreg[ARC_TVS_KSR_MUL + regbase] & 15]));
        // 40+a0+b0:
        double vol_in = (adlibreg[ARC_KSL_OUTLEV + regbase] & 63) +
                                kslmul[adlibreg[ARC_KSL_OUTLEV + regbase] >> 6] * kslev[oct][frn >> 6];
        op_pt.vol = Pow(2.0, vol_in * -0.125 - 14);

        // operator frequency changed, care about features that depend on it
        change_attackrate(regbase, ref op_pt);
        change_decayrate(regbase, ref op_pt);
        change_releaserate(regbase, ref op_pt);
    }

    static void enable_operator(uint regbase, ref op_type op_pt, uint act_type)
    {
        // check if this is really an off-on transition
        if (op_pt.act_state == OP_ACT_OFF)
        {
            int wselbase = (int)regbase;
            if (wselbase >= ARC_SECONDSET) wselbase -= (ARC_SECONDSET - 22);    // second set starts at 22

            op_pt.tcount = wavestart[wave_sel[wselbase]] * FIXEDPT;

            // start with attack mode
            op_pt.op_state = OF_TYPE_ATT;
            op_pt.act_state |= act_type;
        }
    }

    static void disable_operator(ref op_type op_pt, uint act_type)
    {
        // check if this is really an on-off transition
        if (op_pt.act_state != OP_ACT_OFF)
        {
            op_pt.act_state &= (~act_type);
            if (op_pt.act_state == OP_ACT_OFF)
            {
                if (op_pt.op_state != OF_TYPE_OFF) op_pt.op_state = OF_TYPE_REL;
            }
        }
    }

    static bool initfirstime;
    public static void adlib_init(uint samplerate)
    {
        int i, j, oct;

        int_samplerate = (int)samplerate;

        generator_add = (uint)(INTFREQU * FIXEDPT / int_samplerate);


        for (i = 0; i < adlibreg.Length; ++i)
            adlibreg[i] = 0;
        for (i = 0; i < op.Length; ++i)
            op[i] = default;
        for (i = 0; i < wave_sel.Length; ++i)
            wave_sel[i] = 0;

        for (i = 0; i < MAXOPERATORS; i++)
        {
            op[i].op_state = OF_TYPE_OFF;
            op[i].act_state = OP_ACT_OFF;
            op[i].amp = 0.0;
            op[i].step_amp = 0.0;
            op[i].vol = 0.0;
            op[i].tcount = 0;
            op[i].tinc = 0;
            op[i].toff = 0;
            op[i].cur_wmask = wavemask[0];
            op[i].cur_wformIdx = 0;
            op[i].freq_high = 0;

            op[i].generator_pos = 0;
            op[i].cur_env_step = 0;
            op[i].env_step_a = 0;
            op[i].env_step_d = 0;
            op[i].env_step_r = 0;
            op[i].step_skip_pos_a = 0;
            op[i].env_step_skip_a = 0;

#if OPLTYPE_IS_OPL3
		op[i].is_4op = false;
		op[i].is_4op_attached = false;
		op[i].left_pan = 1;
		op[i].right_pan = 1;
#endif
        }

        recipsamp = 1.0 / int_samplerate;
        for (i = 15; i >= 0; i--)
        {
            frqmul[i] = frqmul_tab[i] * INTFREQU / WAVEPREC * FIXEDPT * recipsamp;
        }

        status = 0;
        opl_index = 0;


        // create vibrato table
        vib_table[0] = 8;
        vib_table[1] = 4;
        vib_table[2] = 0;
        vib_table[3] = -4;
        for (i = 4; i < VIBTAB_SIZE; i++) vib_table[i] = vib_table[i - 4] * -1;

        // vibrato at ~6.1 ?? (opl3 docs say 6.1, opl4 docs say 6.0, y8950 docs say 6.4)
        vibtab_add = (uint)(VIBTAB_SIZE * FIXEDPT_LFO / 8192 * INTFREQU / int_samplerate);
        vibtab_pos = 0;

        for (i = 0; i < BLOCKBUF_SIZE; i++) vibval_const[i] = 0;


        // create tremolo table
        int[] trem_table_int = new int[TREMTAB_SIZE];
        for (i = 0; i < 14; i++) trem_table_int[i] = i - 13;        // upwards (13 to 26 . -0.5/6 to 0)
        for (i = 14; i < 41; i++) trem_table_int[i] = -i + 14;      // downwards (26 to 0 . 0 to -1/6)
        for (i = 41; i < 53; i++) trem_table_int[i] = i - 40 - 26;  // upwards (1 to 12 . -1/6 to -0.5/6)

        for (i = 0; i < TREMTAB_SIZE; i++)
        {
            // 0.0 .. -26/26*4.8/6 == [0.0 .. -0.8], 4/53 steps == [1 .. 0.57]
            double trem_val1 = trem_table_int[i] * 4.8 / 26.0 / 6.0;                // 4.8db
            double trem_val2 = trem_table_int[i] / 4 * 1.2 / 6.0 / 6.0;       // 1.2db (larger stepping)

            trem_table[i] = (int)(Pow(2.0, trem_val1) * FIXEDPT);
            trem_table[TREMTAB_SIZE + i] = (int)(Pow(2.0, trem_val2) * FIXEDPT);
        }

        // tremolo at 3.7hz
        tremtab_add = (uint)(TREMTAB_SIZE * TREM_FREQ * FIXEDPT_LFO / int_samplerate);
        tremtab_pos = 0;

        for (i = 0; i < BLOCKBUF_SIZE; i++) tremval_const[i] = FIXEDPT;


        if (!initfirstime)
        {
            initfirstime = true;

            // create waveform tables
            for (i = 0; i < (WAVEPREC >> 1); i++)
            {
                wavtable[(i << 1) + WAVEPREC] = (short)(16384 * Sin((i << 1) * PI * 2 / WAVEPREC));
                wavtable[(i << 1) + 1 + WAVEPREC] = (short)(16384 * Sin(((i << 1) + 1) * PI * 2 / WAVEPREC));
                wavtable[i] = wavtable[(i << 1) + WAVEPREC];
                // alternative: (zero-less)
                /*			wavtable[(i<<1)  +WAVEPREC]	= (short)(16384*sin((double)((i<<2)+1)*PI/WAVEPREC));
                            wavtable[(i<<1)+1+WAVEPREC]	= (short)(16384*sin((double)((i<<2)+3)*PI/WAVEPREC));
                            wavtable[i]					= wavtable[(i<<1)-1+WAVEPREC]; */
            }
            for (i = 0; i < (WAVEPREC >> 3); i++)
            {
                wavtable[i + (WAVEPREC << 1)] = (short)(wavtable[i + (WAVEPREC >> 3)] - 16384);
                wavtable[i + ((WAVEPREC * 17) >> 3)] = (short)(wavtable[i + (WAVEPREC >> 2)] + 16384);
            }

            // key scale level table verified ([table in book]*8/3)
            kslev[7][0] = 0; kslev[7][1] = 24; kslev[7][2] = 32; kslev[7][3] = 37;
            kslev[7][4] = 40; kslev[7][5] = 43; kslev[7][6] = 45; kslev[7][7] = 47;
            kslev[7][8] = 48;
            for (i = 9; i < 16; i++) kslev[7][i] = (byte)(i + 41);
            for (j = 6; j >= 0; j--)
            {
                for (i = 0; i < 16; i++)
                {
                    oct = kslev[j + 1][i] - 8;
                    if (oct < 0) oct = 0;
                    kslev[j][i] = (byte)oct;
                }
            }
        }

    }



    static void adlib_write(uint idx, byte val)
    {
        uint second_set = idx & 0x100;
        adlibreg[idx] = val;

        switch (idx & 0xf0)
        {
            case ARC_CONTROL:
                // here we check for the second set registers, too:
                switch (idx)
                {
                    case 0x02:  // timer1 counter
                    case 0x03:  // timer2 counter
                        break;
                    case 0x04:
                        // IRQ reset, timer mask/start
                        if ((val & 0x80) != 0)
                        {
                            // clear IRQ bits in status register
                            status = (byte)(status & ~0x60);
                        }
                        else
                        {
                            status = 0;
                        }
                        break;
#if OPLTYPE_IS_OPL3
		case 0x04|ARC_SECONDSET:
			// 4op enable/disable switches for each possible channel
			op[0].is_4op = (val&1)>0;
			op[3].is_4op_attached = op[0].is_4op;
			op[1].is_4op = (val&2)>0;
			op[4].is_4op_attached = op[1].is_4op;
			op[2].is_4op = (val&4)>0;
			op[5].is_4op_attached = op[2].is_4op;
			op[18].is_4op = (val&8)>0;
			op[21].is_4op_attached = op[18].is_4op;
			op[19].is_4op = (val&16)>0;
			op[22].is_4op_attached = op[19].is_4op;
			op[20].is_4op = (val&32)>0;
			op[23].is_4op_attached = op[20].is_4op;
			break;
		case 0x05|ARC_SECONDSET:
			break;
#endif
                    case 0x08:
                        // CSW, note select
                        break;
                    default:
                        break;
                }
                break;
            case ARC_TVS_KSR_MUL:
            case ARC_TVS_KSR_MUL + 0x10:
                {
                    // tremolo/vibrato/sustain keeping enabled; key scale rate; frequency multiplication
                    int num = (int)idx & 7;
                    uint base_ = (idx - ARC_TVS_KSR_MUL) & 0xff;
                    if ((num < 6) && (base_ < 22))
                    {
                        uint modop = regbase2modop[second_set != 0 ? (base_ + 22) : base_];
                        uint regbase = base_ + second_set;
                        uint chanbase = second_set != 0 ? (modop - 18 + ARC_SECONDSET) : modop;

                        // change tremolo/vibrato and sustain keeping of this operator
                        op_type op_ptr = op[modop + ((num < 3) ? 0 : 9)];
                        change_keepsustain(regbase, ref op_ptr);
                        change_vibrato(regbase, ref op_ptr);

                        // change frequency calculations of this operator as
                        // key scale rate and frequency multiplicator can be changed
#if OPLTYPE_IS_OPL3
			if ((adlibreg[0x105]&1) && (op[modop].is_4op_attached)) {
				// operator uses frequency of channel
				change_frequency(chanbase-3,regbase,op_ptr);
			} else {
				change_frequency(chanbase,regbase,op_ptr);
			}
#else
                        change_frequency(chanbase, base_, ref op_ptr);
#endif
                        op[modop + ((num < 3) ? 0 : 9)] = op_ptr;
                    }
                }
                break;
            case ARC_KSL_OUTLEV:
            case ARC_KSL_OUTLEV + 0x10:
                {
                    // key scale level; output rate
                    int num = (int)idx & 7;
                    uint base_ = (idx - ARC_KSL_OUTLEV) & 0xff;
                    if ((num < 6) && (base_ < 22))
                    {
                        uint modop = regbase2modop[second_set != 0 ? (base_ + 22) : base_];
                        uint chanbase = second_set != 0 ? (modop - 18 + ARC_SECONDSET) : modop;

                        // change frequency calculations of this operator as
                        // key scale level and output rate can be changed
                        op_type op_ptr = op[modop + ((num < 3) ? 0 : 9)];
#if OPLTYPE_IS_OPL3
			Bitu regbase = base_+second_set;
			if ((adlibreg[0x105]&1) && (op[modop].is_4op_attached)) {
				// operator uses frequency of channel
				change_frequency(chanbase-3,regbase,op_ptr);
			} else {
				change_frequency(chanbase,regbase,op_ptr);
			}
#else
                        change_frequency(chanbase, base_, ref op_ptr);
#endif
                        op[modop + ((num < 3) ? 0 : 9)] = op_ptr;
                    }
                }
                break;
            case ARC_ATTR_DECR:
            case ARC_ATTR_DECR + 0x10:
                {
                    // attack/decay rates
                    int num = (int)idx & 7;
                    uint base_ = (idx - ARC_ATTR_DECR) & 0xff;
                    if ((num < 6) && (base_ < 22))
                    {
                        uint regbase = base_ + second_set;

                        // change attack rate and decay rate of this operator
                        op_type op_ptr = op[regbase2op[second_set != 0 ? (base_ + 22) : base_]];
                        change_attackrate(regbase, ref op_ptr);
                        change_decayrate(regbase, ref op_ptr);
                        op[regbase2op[second_set != 0 ? (base_ + 22) : base_]] = op_ptr;
                    }
                }
                break;
            case ARC_SUSL_RELR:
            case ARC_SUSL_RELR + 0x10:
                {
                    // sustain level; release rate
                    int num = (int)idx & 7;
                    uint base_ = (idx - ARC_SUSL_RELR) & 0xff;
                    if ((num < 6) && (base_ < 22))
                    {
                        uint regbase = base_ + second_set;

                        // change sustain level and release rate of this operator
                        op_type op_ptr = op[regbase2op[second_set != 0 ? (base_ + 22) : base_]];
                        change_releaserate(regbase, ref op_ptr);
                        change_sustainlevel(regbase, ref op_ptr);
                        op[regbase2op[second_set != 0 ? (base_ + 22) : base_]] = op_ptr;
                    }
                }
                break;
            case ARC_FREQ_NUM:
                {
                    // 0xa0-0xa8 low8 frequency
                    uint base_ = (idx - ARC_FREQ_NUM) & 0xff;
                    if (base_ < 9)
                    {
                        int opbase = (int)(second_set != 0 ? (base_ + 18) : base_);
#if OPLTYPE_IS_OPL3
			if ((adlibreg[0x105]&1) && op[opbase].is_4op_attached) break;
#endif
                        // regbase of modulator:
                        uint modbase = modulatorbase[base_] + second_set;

                        uint chanbase = base_ + second_set;

                        change_frequency(chanbase, modbase, ref op[opbase]);
                        change_frequency(chanbase, modbase + 3, ref op[opbase + 9]);
#if OPLTYPE_IS_OPL3
			// for 4op channels all four operators are modified to the frequency of the channel
			if ((adlibreg[0x105]&1) && op[second_set?(base_+18):base_].is_4op) {
				change_frequency(chanbase,modbase+8,ref op[opbase+3]);
				change_frequency(chanbase,modbase+3+8,ref op[opbase+3+9]);
			}
#endif
                    }
                }
                break;
            case ARC_KON_BNUM:
                {
                    if (idx == ARC_PERC_MODE)
                    {
#if OPLTYPE_IS_OPL3
			if (second_set) return;
#endif

                        if ((val & 0x30) == 0x30)
                        {       // BassDrum active
                            enable_operator(16, ref op[6], OP_ACT_PERC);
                            change_frequency(6, 16, ref op[6]);
                            enable_operator(16 + 3, ref op[6 + 9], OP_ACT_PERC);
                            change_frequency(6, 16 + 3, ref op[6 + 9]);
                        }
                        else
                        {
                            disable_operator(ref op[6], OP_ACT_PERC);
                            disable_operator(ref op[6 + 9], OP_ACT_PERC);
                        }
                        if ((val & 0x28) == 0x28)
                        {       // Snare active
                            enable_operator(17 + 3, ref op[16], OP_ACT_PERC);
                            change_frequency(7, 17 + 3, ref op[16]);
                        }
                        else
                        {
                            disable_operator(ref op[16], OP_ACT_PERC);
                        }
                        if ((val & 0x24) == 0x24)
                        {       // TomTom active
                            enable_operator(18, ref op[8], OP_ACT_PERC);
                            change_frequency(8, 18, ref op[8]);
                        }
                        else
                        {
                            disable_operator(ref op[8], OP_ACT_PERC);
                        }
                        if ((val & 0x22) == 0x22)
                        {       // Cymbal active
                            enable_operator(18 + 3, ref op[8 + 9], OP_ACT_PERC);
                            change_frequency(8, 18 + 3, ref op[8 + 9]);
                        }
                        else
                        {
                            disable_operator(ref op[8 + 9], OP_ACT_PERC);
                        }
                        if ((val & 0x21) == 0x21)
                        {       // Hihat active
                            enable_operator(17, ref op[7], OP_ACT_PERC);
                            change_frequency(7, 17, ref op[7]);
                        }
                        else
                        {
                            disable_operator(ref op[7], OP_ACT_PERC);
                        }

                        break;
                    }
                    // regular 0xb0-0xb8
                    uint base_ = (idx - ARC_KON_BNUM) & 0xff;
                    if (base_ < 9)
                    {
                        uint opbase = second_set != 0 ? (base_ + 18) : base_;
#if OPLTYPE_IS_OPL3
			if ((adlibreg[0x105]&1) && op[opbase].is_4op_attached) break;
#endif
                        // regbase of modulator:
                        uint modbase = modulatorbase[base_] + second_set;

                        if ((val & 32) != 0)
                        {
                            // operator switched on
                            enable_operator(modbase, ref op[opbase], OP_ACT_NORMAL);       // modulator (if 2op)
                            enable_operator(modbase + 3, ref op[opbase + 9], OP_ACT_NORMAL);   // carrier (if 2op)
#if OPLTYPE_IS_OPL3
				// for 4op channels all four operators are switched on
				if ((adlibreg[0x105]&1) && op[opbase].is_4op) {
					// turn on chan+3 operators as well
					enable_operator(modbase+8,ref op[opbase+3],OP_ACT_NORMAL);
					enable_operator(modbase+3+8,ref op[opbase+3+9],OP_ACT_NORMAL);
				}
#endif
                        }
                        else
                        {
                            // operator switched off
                            disable_operator(ref op[opbase], OP_ACT_NORMAL);
                            disable_operator(ref op[opbase + 9], OP_ACT_NORMAL);
#if OPLTYPE_IS_OPL3
				// for 4op channels all four operators are switched off
				if ((adlibreg[0x105]&1) && op[opbase].is_4op) {
					// turn off chan+3 operators as well
					disable_operator(ref op[opbase+3],OP_ACT_NORMAL);
					disable_operator(ref op[opbase+3+9],OP_ACT_NORMAL);
				}
#endif
                        }

                        uint chanbase = base_ + second_set;

                        // change frequency calculations of modulator and carrier (2op) as
                        // the frequency of the channel has changed
                        change_frequency(chanbase, modbase, ref op[opbase]);
                        change_frequency(chanbase, modbase + 3, ref op[opbase + 9]);
#if OPLTYPE_IS_OPL3
			// for 4op channels all four operators are modified to the frequency of the channel
			if ((adlibreg[0x105]&1) && op[second_set?(base_+18):base_].is_4op) {
				// change frequency calculations of chan+3 operators as well
				change_frequency(chanbase,modbase+8,ref op[opbase+3]);
				change_frequency(chanbase,modbase+3+8,ref op[opbase+3+9]);
			}
#endif
                    }
                }
                break;
            case ARC_FEEDBACK:
                {
                    // 0xc0-0xc8 feedback/modulation type (AM/FM)
                    uint base_ = (idx - ARC_FEEDBACK) & 0xff;
                    if (base_ < 9)
                    {
                        uint opbase = second_set != 0 ? (base_ + 18) : base_;
                        uint chanbase = base_ + second_set;
                        change_feedback(chanbase, ref op[opbase]);
#if OPLTYPE_IS_OPL3
			// OPL3 panning
			op[opbase].left_pan = ((val&0x10)>>4);
			op[opbase].right_pan = ((val&0x20)>>5);
#endif
                    }
                }
                break;
            case ARC_WAVE_SEL:
            case ARC_WAVE_SEL + 0x10:
                {
                    int num = (int)idx & 7;
                    uint base_ = (idx - ARC_WAVE_SEL) & 0xff;
                    if ((num < 6) && (base_ < 22))
                    {
#if OPLTYPE_IS_OPL3
			Bits wselbase = second_set?(base_+22):base_;	// for easier mapping onto wave_sel[]
			// change waveform
			if (adlibreg[0x105]&1) wave_sel[wselbase] = val&7;	// opl3 mode enabled, all waveforms accessible
			else wave_sel[wselbase] = val&3;
			op_type* op_ptr = ref op[regbase2modop[wselbase]+((num<3) ? 0 : 9)];
			change_waveform(wselbase,op_ptr);
#else
                        if ((adlibreg[0x01] & 0x20) != 0)
                        {
                            // wave selection enabled, change waveform
                            wave_sel[base_] = (byte)(val & 3);
                            op_type op_ptr = op[regbase2modop[base_] + ((num < 3) ? 0 : 9)];
                            change_waveform(base_, ref op_ptr);
                            op[regbase2modop[base_] + ((num < 3) ? 0 : 9)] = op_ptr;
                        }
#endif
                    }
                }
                break;
            default:
                break;
        }
    }


    static uint adlib_reg_read(uint port)
    {
#if OPLTYPE_IS_OPL3
	// opl3-detection routines require ret&6 to be zero
	if ((port&1)==0) {
		return status;
	}
	return 0x00;
#else
        // opl2-detection routines require ret&6 to be 6
        if ((port & 1) == 0)
        {
            return status | 6U;
        }
        return 0xff;
#endif
    }

    static void adlib_write_index(uint port, byte val)
    {
        //(void)port;
        opl_index = val;
#if OPLTYPE_IS_OPL3
	if ((port&3)!=0) {
		// possibly second set
		if (((adlibreg[0x105]&1)!=0) || (opl_index==5)) opl_index |= ARC_SECONDSET;
	}
#endif
    }

    static void clipitf(int ival, float[] outval, int idx)
    {
        if (ival < 32768)
        {
            if (ival > -32769)
            {
                outval[idx] = ival / (float)short.MaxValue;
            }
            else
            {
                outval[idx] = -1;
            }
        }
        else
        {
            outval[idx] = 1;
        }
    }



    // be careful with this
    // uses cptr and chanval, outputs into outbufl(/outbufr)
    // for opl3 check if opl3-mode is enabled (which uses stereo panning)
#if OPLTYPE_IS_OPL3
        TODO: Replace all instances of "outbufl[i] += chanval;" with this:

	if (adlibreg[0x105]&1) {						\
		outbufl[i] += chanval*op[cptrIdx + 0].left_pan;		\
		outbufr[i] += chanval*op[cptrIdx + 0].right_pan;	\
	} else {										\
		outbufl[i] += chanval;						\
	}
#endif

    static int[] outbufl = new int[BLOCKBUF_SIZE];
    static void adlib_getsample(float[] sndptr, int sndptrIdx, int numsamples)
    {
        int i, endsamples;
        int cptrIdx;

#if OPLTYPE_IS_OPL3
	// second output buffer (right channel for opl3 stereo)
	int outbufr[BLOCKBUF_SIZE];
#endif

        // vibrato/tremolo lookup tables (global, to possibly be used by all operators)
        int[] vib_lut = new int[BLOCKBUF_SIZE];
        int[] trem_lut = new int[BLOCKBUF_SIZE];

        int samples_to_process = numsamples;

        for (int cursmp = 0; cursmp < samples_to_process; cursmp += endsamples)
        {
            endsamples = samples_to_process - cursmp;
            if (endsamples > BLOCKBUF_SIZE) endsamples = BLOCKBUF_SIZE;

            for (i = 0; i < endsamples; ++i)
                outbufl[i] = 0;
#if OPLTYPE_IS_OPL3
		// clear second output buffer (opl3 stereo)
		if (adlibreg[0x105]&1) memset((void*)&outbufr,0,endsamples*sizeof(int));
#endif

            // calculate vibrato/tremolo lookup tables
            int vib_tshift = ((adlibreg[ARC_PERC_MODE] & 0x40) == 0) ? 1 : 0;    // 14cents/7cents switching
            for (i = 0; i < endsamples; i++)
            {
                // cycle through vibrato table
                vibtab_pos += vibtab_add;
                if (vibtab_pos / FIXEDPT_LFO >= VIBTAB_SIZE) vibtab_pos -= VIBTAB_SIZE * FIXEDPT_LFO;
                vib_lut[i] = vib_table[vibtab_pos / FIXEDPT_LFO] >> vib_tshift;     // 14cents (14/100 of a semitone) or 7cents

                // cycle through tremolo table
                tremtab_pos += tremtab_add;
                if (tremtab_pos / FIXEDPT_LFO >= TREMTAB_SIZE) tremtab_pos -= TREMTAB_SIZE * FIXEDPT_LFO;
                if ((adlibreg[ARC_PERC_MODE] & 0x80) != 0) trem_lut[i] = trem_table[tremtab_pos / FIXEDPT_LFO];
                else trem_lut[i] = trem_table[TREMTAB_SIZE + tremtab_pos / FIXEDPT_LFO];
            }

            if ((adlibreg[ARC_PERC_MODE] & 0x20) != 0)
            {
                //BassDrum
                cptrIdx = 6;
                if ((adlibreg[ARC_FEEDBACK + 6] & 1) != 0)
                {
                    // additive synthesis
                    if (op[cptrIdx + 9].op_state != OF_TYPE_OFF)
                    {
                        if (op[cptrIdx + 9].vibrato)
                        {
                            vibval1 = vibval_var1;
                            for (i = 0; i < endsamples; i++)
                                vibval1[i] = (int)((vib_lut[i] * op[cptrIdx + 9].freq_high / 8) * FIXEDPT * VIBFAC);
                        }
                        else vibval1 = vibval_const;
                        if (op[cptrIdx + 9].tremolo) tremval1 = trem_lut;   // tremolo enabled, use table
                        else tremval1 = tremval_const;

                        // calculate channel output
                        for (i = 0; i < endsamples; i++)
                        {
                            operator_advance(ref op[cptrIdx + 9], vibval1[i]);
                            opfuncs[op[cptrIdx + 9].op_state](ref op[cptrIdx + 9]);
                            operator_output(ref op[cptrIdx + 9], 0, tremval1[i]);

                            int chanval = op[cptrIdx + 9].cval * 2;
                            outbufl[i] += chanval;

                        }
                    }
                }
                else
                {
                    // frequency modulation
                    if ((op[cptrIdx + 9].op_state != OF_TYPE_OFF) || (op[cptrIdx + 0].op_state != OF_TYPE_OFF))
                    {
                        if ((op[cptrIdx + 0].vibrato) && (op[cptrIdx + 0].op_state != OF_TYPE_OFF))
                        {
                            vibval1 = vibval_var1;
                            for (i = 0; i < endsamples; i++)
                                vibval1[i] = (int)((vib_lut[i] * op[cptrIdx + 0].freq_high / 8) * FIXEDPT * VIBFAC);
                        }
                        else vibval1 = vibval_const;
                        if ((op[cptrIdx + 9].vibrato) && (op[cptrIdx + 9].op_state != OF_TYPE_OFF))
                        {
                            vibval2 = vibval_var2;
                            for (i = 0; i < endsamples; i++)
                                vibval2[i] = (int)((vib_lut[i] * op[cptrIdx + 9].freq_high / 8) * FIXEDPT * VIBFAC);
                        }
                        else vibval2 = vibval_const;
                        if (op[cptrIdx + 0].tremolo) tremval1 = trem_lut;   // tremolo enabled, use table
                        else tremval1 = tremval_const;
                        if (op[cptrIdx + 9].tremolo) tremval2 = trem_lut;   // tremolo enabled, use table
                        else tremval2 = tremval_const;

                        // calculate channel output
                        for (i = 0; i < endsamples; i++)
                        {
                            operator_advance(ref op[cptrIdx + 0], vibval1[i]);
                            opfuncs[op[cptrIdx + 0].op_state](ref op[cptrIdx + 0]);
                            operator_output(ref op[cptrIdx + 0], (op[cptrIdx + 0].lastcval + op[cptrIdx + 0].cval) * op[cptrIdx + 0].mfbi / 2, tremval1[i]);

                            operator_advance(ref op[cptrIdx + 9], vibval2[i]);
                            opfuncs[op[cptrIdx + 9].op_state](ref op[cptrIdx + 9]);
                            operator_output(ref op[cptrIdx + 9], op[cptrIdx + 0].cval * FIXEDPT, tremval2[i]);

                            int chanval = op[cptrIdx + 9].cval * 2;
                            outbufl[i] += chanval;

                        }
                    }
                }

                //TomTom (j=8)
                if (op[8].op_state != OF_TYPE_OFF)
                {
                    cptrIdx = 8;
                    if (op[cptrIdx + 0].vibrato)
                    {
                        vibval3 = vibval_var1;
                        for (i = 0; i < endsamples; i++)
                            vibval3[i] = (int)((vib_lut[i] * op[cptrIdx + 0].freq_high / 8) * FIXEDPT * VIBFAC);
                    }
                    else vibval3 = vibval_const;

                    if (op[cptrIdx + 0].tremolo) tremval3 = trem_lut;   // tremolo enabled, use table
                    else tremval3 = tremval_const;

                    // calculate channel output
                    for (i = 0; i < endsamples; i++)
                    {
                        operator_advance(ref op[cptrIdx + 0], vibval3[i]);
                        opfuncs[op[cptrIdx + 0].op_state](ref op[cptrIdx + 0]);        //TomTom
                        operator_output(ref op[cptrIdx + 0], 0, tremval3[i]);
                        int chanval = op[cptrIdx + 0].cval * 2;
                        outbufl[i] += chanval;

                    }
                }

                //Snare/Hihat (j=7), Cymbal (j=8)
                if ((op[7].op_state != OF_TYPE_OFF) || (op[16].op_state != OF_TYPE_OFF) ||
                    (op[17].op_state != OF_TYPE_OFF))
                {
                    cptrIdx = 7;
                    if ((op[cptrIdx + 0].vibrato) && (op[cptrIdx + 0].op_state != OF_TYPE_OFF))
                    {
                        vibval1 = vibval_var1;
                        for (i = 0; i < endsamples; i++)
                            vibval1[i] = (int)((vib_lut[i] * op[cptrIdx + 0].freq_high / 8) * FIXEDPT * VIBFAC);
                    }
                    else vibval1 = vibval_const;
                    if ((op[cptrIdx + 9].vibrato) && (op[cptrIdx + 9].op_state == OF_TYPE_OFF))
                    {
                        vibval2 = vibval_var2;
                        for (i = 0; i < endsamples; i++)
                            vibval2[i] = (int)((vib_lut[i] * op[cptrIdx + 9].freq_high / 8) * FIXEDPT * VIBFAC);
                    }
                    else vibval2 = vibval_const;

                    if (op[cptrIdx + 0].tremolo) tremval1 = trem_lut;   // tremolo enabled, use table
                    else tremval1 = tremval_const;
                    if (op[cptrIdx + 9].tremolo) tremval2 = trem_lut;   // tremolo enabled, use table
                    else tremval2 = tremval_const;

                    cptrIdx = 8;
                    if ((op[cptrIdx + 9].vibrato) && (op[cptrIdx + 9].op_state == OF_TYPE_OFF))
                    {
                        vibval4 = vibval_var2;
                        for (i = 0; i < endsamples; i++)
                            vibval4[i] = (int)((vib_lut[i] * op[cptrIdx + 9].freq_high / 8) * FIXEDPT * VIBFAC);
                    }
                    else vibval4 = vibval_const;

                    if (op[cptrIdx + 9].tremolo) tremval4 = trem_lut;   // tremolo enabled, use table
                    else tremval4 = tremval_const;

                    // calculate channel output
                    for (i = 0; i < endsamples; i++)
                    {
                        operator_advance_drums(ref op[7], vibval1[i], ref op[7 + 9], vibval2[i], ref op[8 + 9], vibval4[i]);

                        opfuncs[op[7].op_state](ref op[7]);            //Hihat
                        operator_output(ref op[7], 0, tremval1[i]);

                        opfuncs[op[7 + 9].op_state](ref op[7 + 9]);        //Snare
                        operator_output(ref op[7 + 9], 0, tremval2[i]);

                        opfuncs[op[8 + 9].op_state](ref op[8 + 9]);        //Cymbal
                        operator_output(ref op[8 + 9], 0, tremval4[i]);

                        int chanval = (op[7].cval + op[7 + 9].cval + op[8 + 9].cval) * 2;
                        outbufl[i] += chanval;

                    }
                }
            }

            int max_channel = NUM_CHANNELS;
#if OPLTYPE_IS_OPL3
		if ((adlibreg[0x105]&1)==0) max_channel = NUM_CHANNELS/2;
#endif
            for (int cur_ch = max_channel - 1; cur_ch >= 0; cur_ch--)
            {
                // skip drum/percussion operators
                if ((adlibreg[ARC_PERC_MODE] & 0x20) != 0 && (cur_ch >= 6) && (cur_ch < 9)) continue;

                int k = cur_ch;
#if OPLTYPE_IS_OPL3
			if (cur_ch < 9) {
				cptr = ref op[cur_ch];
			} else {
				cptr = ref op[cur_ch+9];	// second set is operator18-operator35
				k += (-9+256);		// second set uses registers 0x100 onwards
			}
			// check if this operator is part of a 4-op
			if ((adlibreg[0x105]&1) && cptr.is_4op_attached) continue;
#else
                cptrIdx = cur_ch;
#endif

                // check for FM/AM
                if ((adlibreg[ARC_FEEDBACK + k] & 1) != 0)
                {
#if OPLTYPE_IS_OPL3
				if ((adlibreg[0x105]&1) && cptr.is_4op) {
					if (adlibreg[ARC_FEEDBACK+k+3]&1) {
						// AM-AM-style synthesis (op1[fb] + (op2 * op3) + op4)
						if (op[cptrIdx + 0].op_state != OF_TYPE_OFF) {
							if (op[cptrIdx + 0].vibrato) {
								vibval1 = vibval_var1;
								for (i=0;i<endsamples;i++)
									vibval1[i] = (int)((vib_lut[i]*op[cptrIdx + 0].freq_high/8)*FIXEDPT*VIBFAC);
							} else vibval1 = vibval_const;
							if (op[cptrIdx + 0].tremolo) tremval1 = trem_lut;	// tremolo enabled, use table
							else tremval1 = tremval_const;

							// calculate channel output
							for (i=0;i<endsamples;i++) {
								operator_advance(ref op[cptrIdx + 0],vibval1[i]);
								opfuncs[op[cptrIdx + 0].op_state](ref op[cptrIdx + 0]);
								operator_output(ref op[cptrIdx + 0],(op[cptrIdx + 0].lastcval+op[cptrIdx + 0].cval)*op[cptrIdx + 0].mfbi/2,tremval1[i]);

								int chanval = op[cptrIdx + 0].cval;
								outbufl[i] += chanval;
							}
						}

						if ((op[cptrIdx + 3].op_state != OF_TYPE_OFF) || (op[cptrIdx + 9].op_state != OF_TYPE_OFF)) {
							if ((op[cptrIdx + 9].vibrato) && (op[cptrIdx + 9].op_state != OF_TYPE_OFF)) {
								vibval1 = vibval_var1;
								for (i=0;i<endsamples;i++)
									vibval1[i] = (int)((vib_lut[i]*op[cptrIdx + 9].freq_high/8)*FIXEDPT*VIBFAC);
							} else vibval1 = vibval_const;
							if (op[cptrIdx + 9].tremolo) tremval1 = trem_lut;	// tremolo enabled, use table
							else tremval1 = tremval_const;
							if (op[cptrIdx + 3].tremolo) tremval2 = trem_lut;	// tremolo enabled, use table
							else tremval2 = tremval_const;

							// calculate channel output
							for (i=0;i<endsamples;i++) {
								operator_advance(ref op[cptrIdx + 9],vibval1[i]);
								opfuncs[op[cptrIdx + 9].op_state](ref op[cptrIdx + 9]);
								operator_output(ref op[cptrIdx + 9],0,tremval1[i]);

								operator_advance(ref op[cptrIdx + 3],0);
								opfuncs[op[cptrIdx + 3].op_state](ref op[cptrIdx + 3]);
								operator_output(ref op[cptrIdx + 3],op[cptrIdx + 9].cval*FIXEDPT,tremval2[i]);

								int chanval = op[cptrIdx + 3].cval;
								outbufl[i] += chanval;
							}
						}

						if (op[cptrIdx + 3+9].op_state != OF_TYPE_OFF) {
							if (op[cptrIdx + 3+9].tremolo) tremval1 = trem_lut;	// tremolo enabled, use table
							else tremval1 = tremval_const;

							// calculate channel output
							for (i=0;i<endsamples;i++) {
								operator_advance(ref op[cptrIdx + 3+9],0);
								opfuncs[op[cptrIdx + 3+9].op_state](ref op[cptrIdx + 3+9]);
								operator_output(ref op[cptrIdx + 3+9],0,tremval1[i]);

								int chanval = op[cptrIdx + 3+9].cval;
								outbufl[i] += chanval;
							}
						}
					} else {
						// AM-FM-style synthesis (op1[fb] + (op2 * op3 * op4))
						if (op[cptrIdx + 0].op_state != OF_TYPE_OFF) {
							if (op[cptrIdx + 0].vibrato) {
								vibval1 = vibval_var1;
								for (i=0;i<endsamples;i++)
									vibval1[i] = (int)((vib_lut[i]*op[cptrIdx + 0].freq_high/8)*FIXEDPT*VIBFAC);
							} else vibval1 = vibval_const;
							if (op[cptrIdx + 0].tremolo) tremval1 = trem_lut;	// tremolo enabled, use table
							else tremval1 = tremval_const;

							// calculate channel output
							for (i=0;i<endsamples;i++) {
								operator_advance(ref op[cptrIdx + 0],vibval1[i]);
								opfuncs[op[cptrIdx + 0].op_state](ref op[cptrIdx + 0]);
								operator_output(ref op[cptrIdx + 0],(op[cptrIdx + 0].lastcval+op[cptrIdx + 0].cval)*op[cptrIdx + 0].mfbi/2,tremval1[i]);

								int chanval = op[cptrIdx + 0].cval;
								outbufl[i] += chanval;
							}
						}

						if ((op[cptrIdx + 9].op_state != OF_TYPE_OFF) || (op[cptrIdx + 3].op_state != OF_TYPE_OFF) || (op[cptrIdx + 3+9].op_state != OF_TYPE_OFF)) {
							if ((op[cptrIdx + 9].vibrato) && (op[cptrIdx + 9].op_state != OF_TYPE_OFF)) {
								vibval1 = vibval_var1;
								for (i=0;i<endsamples;i++)
									vibval1[i] = (int)((vib_lut[i]*op[cptrIdx + 9].freq_high/8)*FIXEDPT*VIBFAC);
							} else vibval1 = vibval_const;
							if (op[cptrIdx + 9].tremolo) tremval1 = trem_lut;	// tremolo enabled, use table
							else tremval1 = tremval_const;
							if (op[cptrIdx + 3].tremolo) tremval2 = trem_lut;	// tremolo enabled, use table
							else tremval2 = tremval_const;
							if (op[cptrIdx + 3+9].tremolo) tremval3 = trem_lut;	// tremolo enabled, use table
							else tremval3 = tremval_const;

							// calculate channel output
							for (i=0;i<endsamples;i++) {
								operator_advance(ref op[cptrIdx + 9],vibval1[i]);
								opfuncs[op[cptrIdx + 9].op_state](ref op[cptrIdx + 9]);
								operator_output(ref op[cptrIdx + 9],0,tremval1[i]);

								operator_advance(ref op[cptrIdx + 3],0);
								opfuncs[op[cptrIdx + 3].op_state](ref op[cptrIdx + 3]);
								operator_output(ref op[cptrIdx + 3],op[cptrIdx + 9].cval*FIXEDPT,tremval2[i]);

								operator_advance(ref op[cptrIdx + 3+9],0);
								opfuncs[op[cptrIdx + 3+9].op_state](ref op[cptrIdx + 3+9]);
								operator_output(ref op[cptrIdx + 3+9],op[cptrIdx + 3].cval*FIXEDPT,tremval3[i]);

								int chanval = op[cptrIdx + 3+9].cval;
								outbufl[i] += chanval;
							}
						}
					}
					continue;
				}
#endif
                    // 2op additive synthesis
                    if ((op[cptrIdx + 9].op_state == OF_TYPE_OFF) && (op[cptrIdx + 0].op_state == OF_TYPE_OFF)) continue;
                    if ((op[cptrIdx + 0].vibrato) && (op[cptrIdx + 0].op_state != OF_TYPE_OFF))
                    {
                        vibval1 = vibval_var1;
                        for (i = 0; i < endsamples; i++)
                            vibval1[i] = (int)((vib_lut[i] * op[cptrIdx + 0].freq_high / 8) * FIXEDPT * VIBFAC);
                    }
                    else vibval1 = vibval_const;
                    if ((op[cptrIdx + 9].vibrato) && (op[cptrIdx + 9].op_state != OF_TYPE_OFF))
                    {
                        vibval2 = vibval_var2;
                        for (i = 0; i < endsamples; i++)
                            vibval2[i] = (int)((vib_lut[i] * op[cptrIdx + 9].freq_high / 8) * FIXEDPT * VIBFAC);
                    }
                    else vibval2 = vibval_const;
                    if (op[cptrIdx + 0].tremolo) tremval1 = trem_lut;   // tremolo enabled, use table
                    else tremval1 = tremval_const;
                    if (op[cptrIdx + 9].tremolo) tremval2 = trem_lut;   // tremolo enabled, use table
                    else tremval2 = tremval_const;

                    // calculate channel output
                    for (i = 0; i < endsamples; i++)
                    {
                        // carrier1
                        operator_advance(ref op[cptrIdx + 0], vibval1[i]);
                        opfuncs[op[cptrIdx + 0].op_state](ref op[cptrIdx + 0]);
                        operator_output(ref op[cptrIdx + 0], (op[cptrIdx + 0].lastcval + op[cptrIdx + 0].cval) * op[cptrIdx + 0].mfbi / 2, tremval1[i]);

                        // carrier2
                        operator_advance(ref op[cptrIdx + 9], vibval2[i]);
                        opfuncs[op[cptrIdx + 9].op_state](ref op[cptrIdx + 9]);
                        operator_output(ref op[cptrIdx + 9], 0, tremval2[i]);

                        int chanval = op[cptrIdx + 9].cval + op[cptrIdx + 0].cval;
                        outbufl[i] += chanval;

                    }
                }
                else
                {
#if OPLTYPE_IS_OPL3
				if ((adlibreg[0x105]&1) && cptr.is_4op) {
					if (adlibreg[ARC_FEEDBACK+k+3]&1) {
						// FM-AM-style synthesis ((op1[fb] * op2) + (op3 * op4))
						if ((op[cptrIdx + 0].op_state != OF_TYPE_OFF) || (op[cptrIdx + 9].op_state != OF_TYPE_OFF)) {
							if ((op[cptrIdx + 0].vibrato) && (op[cptrIdx + 0].op_state != OF_TYPE_OFF)) {
								vibval1 = vibval_var1;
								for (i=0;i<endsamples;i++)
									vibval1[i] = (int)((vib_lut[i]*op[cptrIdx + 0].freq_high/8)*FIXEDPT*VIBFAC);
							} else vibval1 = vibval_const;
							if ((op[cptrIdx + 9].vibrato) && (op[cptrIdx + 9].op_state != OF_TYPE_OFF)) {
								vibval2 = vibval_var2;
								for (i=0;i<endsamples;i++)
									vibval2[i] = (int)((vib_lut[i]*op[cptrIdx + 9].freq_high/8)*FIXEDPT*VIBFAC);
							} else vibval2 = vibval_const;
							if (op[cptrIdx + 0].tremolo) tremval1 = trem_lut;	// tremolo enabled, use table
							else tremval1 = tremval_const;
							if (op[cptrIdx + 9].tremolo) tremval2 = trem_lut;	// tremolo enabled, use table
							else tremval2 = tremval_const;

							// calculate channel output
							for (i=0;i<endsamples;i++) {
								operator_advance(ref op[cptrIdx + 0],vibval1[i]);
								opfuncs[op[cptrIdx + 0].op_state](ref op[cptrIdx + 0]);
								operator_output(ref op[cptrIdx + 0],(op[cptrIdx + 0].lastcval+op[cptrIdx + 0].cval)*op[cptrIdx + 0].mfbi/2,tremval1[i]);

								operator_advance(ref op[cptrIdx + 9],vibval2[i]);
								opfuncs[op[cptrIdx + 9].op_state](ref op[cptrIdx + 9]);
								operator_output(ref op[cptrIdx + 9],op[cptrIdx + 0].cval*FIXEDPT,tremval2[i]);

								int chanval = op[cptrIdx + 9].cval;
								outbufl[i] += chanval;
							}
						}

						if ((op[cptrIdx + 3].op_state != OF_TYPE_OFF) || (op[cptrIdx + 3+9].op_state != OF_TYPE_OFF)) {
							if (op[cptrIdx + 3].tremolo) tremval1 = trem_lut;	// tremolo enabled, use table
							else tremval1 = tremval_const;
							if (op[cptrIdx + 3+9].tremolo) tremval2 = trem_lut;	// tremolo enabled, use table
							else tremval2 = tremval_const;

							// calculate channel output
							for (i=0;i<endsamples;i++) {
								operator_advance(ref op[cptrIdx + 3],0);
								opfuncs[op[cptrIdx + 3].op_state](ref op[cptrIdx + 3]);
								operator_output(ref op[cptrIdx + 3],0,tremval1[i]);

								operator_advance(ref op[cptrIdx + 3+9],0);
								opfuncs[op[cptrIdx + 3+9].op_state](ref op[cptrIdx + 3+9]);
								operator_output(ref op[cptrIdx + 3+9],op[cptrIdx + 3].cval*FIXEDPT,tremval2[i]);

								int chanval = op[cptrIdx + 3+9].cval;
								outbufl[i] += chanval;
							}
						}

					} else {
						// FM-FM-style synthesis (op1[fb] * op2 * op3 * op4)
						if ((op[cptrIdx + 0].op_state != OF_TYPE_OFF) || (op[cptrIdx + 9].op_state != OF_TYPE_OFF) || 
							(op[cptrIdx + 3].op_state != OF_TYPE_OFF) || (op[cptrIdx + 3+9].op_state != OF_TYPE_OFF)) {
							if ((op[cptrIdx + 0].vibrato) && (op[cptrIdx + 0].op_state != OF_TYPE_OFF)) {
								vibval1 = vibval_var1;
								for (i=0;i<endsamples;i++)
									vibval1[i] = (int)((vib_lut[i]*op[cptrIdx + 0].freq_high/8)*FIXEDPT*VIBFAC);
							} else vibval1 = vibval_const;
							if ((op[cptrIdx + 9].vibrato) && (op[cptrIdx + 9].op_state != OF_TYPE_OFF)) {
								vibval2 = vibval_var2;
								for (i=0;i<endsamples;i++)
									vibval2[i] = (int)((vib_lut[i]*op[cptrIdx + 9].freq_high/8)*FIXEDPT*VIBFAC);
							} else vibval2 = vibval_const;
							if (op[cptrIdx + 0].tremolo) tremval1 = trem_lut;	// tremolo enabled, use table
							else tremval1 = tremval_const;
							if (op[cptrIdx + 9].tremolo) tremval2 = trem_lut;	// tremolo enabled, use table
							else tremval2 = tremval_const;
							if (op[cptrIdx + 3].tremolo) tremval3 = trem_lut;	// tremolo enabled, use table
							else tremval3 = tremval_const;
							if (op[cptrIdx + 3+9].tremolo) tremval4 = trem_lut;	// tremolo enabled, use table
							else tremval4 = tremval_const;

							// calculate channel output
							for (i=0;i<endsamples;i++) {
								operator_advance(ref op[cptrIdx + 0],vibval1[i]);
								opfuncs[op[cptrIdx + 0].op_state](ref op[cptrIdx + 0]);
								operator_output(ref op[cptrIdx + 0],(op[cptrIdx + 0].lastcval+op[cptrIdx + 0].cval)*op[cptrIdx + 0].mfbi/2,tremval1[i]);

								operator_advance(ref op[cptrIdx + 9],vibval2[i]);
								opfuncs[op[cptrIdx + 9].op_state](ref op[cptrIdx + 9]);
								operator_output(ref op[cptrIdx + 9],op[cptrIdx + 0].cval*FIXEDPT,tremval2[i]);

								operator_advance(ref op[cptrIdx + 3],0);
								opfuncs[op[cptrIdx + 3].op_state](ref op[cptrIdx + 3]);
								operator_output(ref op[cptrIdx + 3],op[cptrIdx + 9].cval*FIXEDPT,tremval3[i]);

								operator_advance(ref op[cptrIdx + 3+9],0);
								opfuncs[op[cptrIdx + 3+9].op_state](ref op[cptrIdx + 3+9]);
								operator_output(ref op[cptrIdx + 3+9],op[cptrIdx + 3].cval*FIXEDPT,tremval4[i]);

								int chanval = op[cptrIdx + 3+9].cval;
								outbufl[i] += chanval;
							}
						}
					}
					continue;
				}
#endif
                    // 2op frequency modulation
                    if ((op[cptrIdx + 9].op_state == OF_TYPE_OFF) && (op[cptrIdx + 0].op_state == OF_TYPE_OFF)) continue;
                    if ((op[cptrIdx + 0].vibrato) && (op[cptrIdx + 0].op_state != OF_TYPE_OFF))
                    {
                        vibval1 = vibval_var1;
                        for (i = 0; i < endsamples; i++)
                            vibval1[i] = (int)((vib_lut[i] * op[cptrIdx + 0].freq_high / 8) * FIXEDPT * VIBFAC);
                    }
                    else vibval1 = vibval_const;
                    if ((op[cptrIdx + 9].vibrato) && (op[cptrIdx + 9].op_state != OF_TYPE_OFF))
                    {
                        vibval2 = vibval_var2;
                        for (i = 0; i < endsamples; i++)
                            vibval2[i] = (int)((vib_lut[i] * op[cptrIdx + 9].freq_high / 8) * FIXEDPT * VIBFAC);
                    }
                    else vibval2 = vibval_const;
                    if (op[cptrIdx + 0].tremolo) tremval1 = trem_lut;   // tremolo enabled, use table
                    else tremval1 = tremval_const;
                    if (op[cptrIdx + 9].tremolo) tremval2 = trem_lut;   // tremolo enabled, use table
                    else tremval2 = tremval_const;

                    // calculate channel output
                    for (i = 0; i < endsamples; i++)
                    {
                        // modulator
                        operator_advance(ref op[cptrIdx + 0], vibval1[i]);
                        opfuncs[op[cptrIdx + 0].op_state](ref op[cptrIdx + 0]);
                        operator_output(ref op[cptrIdx + 0], (op[cptrIdx + 0].lastcval + op[cptrIdx + 0].cval) * op[cptrIdx + 0].mfbi / 2, tremval1[i]);

                        // carrier
                        operator_advance(ref op[cptrIdx + 9], vibval2[i]);
                        opfuncs[op[cptrIdx + 9].op_state](ref op[cptrIdx + 9]);
                        operator_output(ref op[cptrIdx + 9], op[cptrIdx + 0].cval * FIXEDPT, tremval2[i]);

                        int chanval = op[cptrIdx + 9].cval;
                        outbufl[i] += chanval;

                    }
                }
            }

#if OPLTYPE_IS_OPL3
		if (adlibreg[0x105]&1) {
			// convert to 16bit samples (stereo)
			for (i=0;i<endsamples;i++) {
				clipit16(outbufl[i],sndptr++);
				clipit16(outbufr[i],sndptr++);
			}
		} else {
			// convert to 16bit samples (mono)
			for (i=0;i<endsamples;i++) {
				clipit16(outbufl[i],sndptr++);
				clipit16(outbufl[i],sndptr++);
			}
		}
#else
            // convert to floating point samples
            for (i = 0; i < endsamples; i++)
                clipitf(outbufl[i], sndptr, sndptrIdx++);
#endif

        }
    }

}