using System.IO;
using static System.Console;

public static class LdsPlayC
{
    private static readonly byte[] op_table = { 0x00, 0x01, 0x02, 0x08, 0x09, 0x0a, 0x10, 0x11, 0x12 };

    class SoundBank {
        public byte mod_misc, mod_vol, mod_ad, mod_sr, mod_wave,
            car_misc, car_vol, car_ad, car_sr, car_wave, feedback, keyoff,
            portamento, glide, finetune, vibrato, vibdelay, mod_trem, car_trem,
            tremwait, arpeggio;
        public byte[] arp_tab;
        public ushort start, size;
        public byte fms;
        public ushort transp;
        public byte midinst, midvelo, midkey, midtrans, middum1, middum2;
    }

    class Channel {
        public ushort gototune, lasttune, packpos;
        public byte finetune, glideto, portspeed, nextvol, volmod, volcar,
            vibwait, vibspeed, vibrate, trmstay, trmwait, trmspeed, trmrate, trmcount,
            trcwait, trcspeed, trcrate, trccount, arp_size, arp_speed, keycount,
            vibcount, arp_pos, arp_count, packwait;
        public byte[] arp_tab = new byte[12];
        public struct ChanCheat {
            public byte chandelay, sound;
            public ushort high;
        };
        public ChanCheat chancheat;
    }

    struct Position {
        public ushort patnum;
        public byte transpose;
    }
    
    /* A substantial amount of this code has been copied and adapted from adplug.
       Thanks, guys! Adplug is awesome! :D */

    /* Note frequency table (16 notes / octave) */
    private static readonly ushort[] frequency = {
  343, 344, 345, 347, 348, 349, 350, 352, 353, 354, 356, 357, 358,
  359, 361, 362, 363, 365, 366, 367, 369, 370, 371, 373, 374, 375,
  377, 378, 379, 381, 382, 384, 385, 386, 388, 389, 391, 392, 393,
  395, 396, 398, 399, 401, 402, 403, 405, 406, 408, 409, 411, 412,
  414, 415, 417, 418, 420, 421, 423, 424, 426, 427, 429, 430, 432,
  434, 435, 437, 438, 440, 442, 443, 445, 446, 448, 450, 451, 453,
  454, 456, 458, 459, 461, 463, 464, 466, 468, 469, 471, 473, 475,
  476, 478, 480, 481, 483, 485, 487, 488, 490, 492, 494, 496, 497,
  499, 501, 503, 505, 506, 508, 510, 512, 514, 516, 518, 519, 521,
  523, 525, 527, 529, 531, 533, 535, 537, 538, 540, 542, 544, 546,
  548, 550, 552, 554, 556, 558, 560, 562, 564, 566, 568, 571, 573,
  575, 577, 579, 581, 583, 585, 587, 589, 591, 594, 596, 598, 600,
  602, 604, 607, 609, 611, 613, 615, 618, 620, 622, 624, 627, 629,
  631, 633, 636, 638, 640, 643, 645, 647, 650, 652, 654, 657, 659,
  662, 664, 666, 669, 671, 674, 676, 678, 681, 683
};

    /* Vibrato (sine) table */
    private static readonly byte[] vibtab = {
  0, 13, 25, 37, 50, 62, 74, 86, 98, 109, 120, 131, 142, 152, 162,
  171, 180, 189, 197, 205, 212, 219, 225, 231, 236, 240, 244, 247,
  250, 252, 254, 255, 255, 255, 254, 252, 250, 247, 244, 240, 236,
  231, 225, 219, 212, 205, 197, 189, 180, 171, 162, 152, 142, 131,
  120, 109, 98, 86, 74, 62, 50, 37, 25, 13
};

    /* Tremolo (sine * sine) table */
    private static readonly byte[] tremtab = {
  0, 0, 1, 1, 2, 4, 5, 7, 10, 12, 15, 18, 21, 25, 29, 33, 37, 42, 47,
  52, 57, 62, 67, 73, 79, 85, 90, 97, 103, 109, 115, 121, 128, 134,
  140, 146, 152, 158, 165, 170, 176, 182, 188, 193, 198, 203, 208,
  213, 218, 222, 226, 230, 234, 237, 240, 243, 245, 248, 250, 251,
  253, 254, 254, 255, 255, 255, 254, 254, 253, 251, 250, 248, 245,
  243, 240, 237, 234, 230, 226, 222, 218, 213, 208, 203, 198, 193,
  188, 182, 176, 170, 165, 158, 152, 146, 140, 134, 127, 121, 115,
  109, 103, 97, 90, 85, 79, 73, 67, 62, 57, 52, 47, 42, 37, 33, 29,
  25, 21, 18, 15, 12, 10, 7, 5, 4, 2, 1, 1, 0
};

    private const ushort maxsound = 0x3f, maxpos = 0xff;

    private static SoundBank[] soundbank = null;
    private static Channel[] channel = new Channel[9];
    private static Position[] positions = null;

    private static byte[] fmchip = new byte[0xff], chandelay;
    private static byte jumping, fadeonoff, allvolume, hardfade, tempo_now, pattplay, tempo, regbd, mode, pattlen;
    private static ushort posplay, jumppos, speed;
    private static ushort[] patterns = null;
    private static ushort numpatch, numposi, mainvolume;


    //public static bool playing, songlooped;
    public static bool playing => LoudnessC.IntroPlayer.isPlaying || LoudnessC.LoopPlayer.isPlaying;
    public static double loopDspTime;
    public static bool songlooped => UnityEngine.AudioSettings.dspTime > loopDspTime;

    public const float REFRESH = 70.0f;

    private static void lds_setregs(int reg, int val)
    {
        if (fmchip[reg] == (byte)val) return;

        fmchip[reg] = (byte)val;

        //opl_write(reg, val);
    }

    private static void lds_setregs_adv(int reg, int mask, int val)
    {
        lds_setregs(reg, fmchip[reg] & (byte)(mask) | (byte)val);
    }

    public static bool lds_update()
    {
        ushort comword, freq, octave, chan, tune, wibc, tremc, arpreg;
        bool vbreak;
        byte level, regnum, comhi, comlo;
        int i;
        Channel c;

        if (!playing) return false;

        /* handle fading */
        if (fadeonoff > 0)
        {
            if (fadeonoff <= 128)
            {
                if (allvolume > fadeonoff || allvolume == 0)
                {
                    allvolume -= fadeonoff;
                }
                else
                {
                    allvolume = 1;
                    fadeonoff = 0;
                    if (hardfade != 0)
                    {
                        //ED TODO: Music support
                        //playing = false;
                        hardfade = 0;
                        for (i = 0; i < 9; i++)
                        {
                            channel[i].keycount = 1;
                        }
                    }
                }

            }
            else
            {
                if ((byte)((allvolume + (0x100 - fadeonoff)) & 0xff) <= mainvolume)
                {
                    allvolume += (byte)(0x100 - fadeonoff);
                }
                else
                {
                    allvolume = (byte)mainvolume;
                    fadeonoff = 0;
                }
            }
        }

        /* handle channel delay */
        for (chan = 0; chan < 9; chan++)
        {
            c = channel[chan];
            if (c.chancheat.chandelay > 0)
            {
                if (!(--c.chancheat.chandelay > 0))
                {
                    lds_playsound(c.chancheat.sound, chan, c.chancheat.high);
                }
            }
        }

        /* handle notes */
        if (tempo_now == 0 && positions != null)
        {
            vbreak = false;
            for (chan = 0; chan < 9; chan++)
            {
                c = channel[chan];
                if (!(c.packwait > 0))
                {
                    ushort patnum = positions[posplay * 9 + chan].patnum;
                    byte transpose = positions[posplay * 9 + chan].transpose;
                    /*printf("> %p", positions);*/

                    comword = patterns[patnum + c.packpos];
                    comhi = (byte)(comword >> 8); comlo = (byte)(comword & 0xff);
                    if (comword > 0)
                    {
                        if (comhi == 0x80)
                        {
                            c.packwait = comlo;
                        }
                        else
                        {
                            if (comhi >= 0x80)
                            {
                                switch (comhi)
                                {
                                    case 0xff:
                                        c.volcar = (byte)((((c.volcar & 0x3f) * comlo) >> 6) & 0x3f);
                                        if ((fmchip[0xc0 + chan] & 1) != 0)
                                            c.volmod = (byte)((((c.volmod & 0x3f) * comlo) >> 6) & 0x3f);
                                        break;

                                    case 0xfe:
                                        tempo = (byte)(comword & 0x3f);
                                        break;

                                    case 0xfd:
                                        c.nextvol = comlo;
                                        break;

                                    case 0xfc:
                                        //ED TODO: Music support
                                        //playing = false;
                                        /* in real player there's also full keyoff here, but we don't need it */
                                        break;

                                    case 0xfb:
                                        c.keycount = 1;
                                        break;

                                    case 0xfa:
                                        vbreak = true;
                                        jumppos = (ushort)((posplay + 1) & maxpos);
                                        break;

                                    case 0xf9:
                                        vbreak = true;
                                        jumppos = (ushort)(comlo & maxpos);
                                        jumping = 1;
                                        if (jumppos < posplay)
                                        {
                                            //ED TODO: Music support
                                            //songlooped = true;
                                        }
                                        break;

                                    case 0xf8:
                                        c.lasttune = 0;
                                        break;

                                    case 0xf7:
                                        c.vibwait = 0;
                                        /* PASCAL: c.vibspeed = ((comlo >> 4) & 15) + 2; */
                                        c.vibspeed = (byte)((comlo >> 4) + 2);
                                        c.vibrate = (byte)((comlo & 15) + 1);
                                        break;

                                    case 0xf6:
                                        c.glideto = comlo;
                                        break;

                                    case 0xf5:
                                        c.finetune = comlo;
                                        break;

                                    case 0xf4:
                                        if (hardfade != 0)
                                        {
                                            allvolume = (byte)(mainvolume = comlo);
                                            fadeonoff = 0;
                                        }
                                        break;

                                    case 0xf3:
                                        if (hardfade != 0)
                                        {
                                            fadeonoff = comlo;
                                        }
                                        break;

                                    case 0xf2:
                                        c.trmstay = comlo;
                                        break;

                                    case 0xf1:  /* panorama */

                                    case 0xf0:  /* progch */
                                                /* MIDI commands (unhandled) */
                                                /*AdPlug_LogWrite("CldsPlayer(): not handling MIDI command 0x%x, "
                                                    "value = 0x%x\n", comhi);*/
                                        break;

                                    default:
                                        if (comhi < 0xa0)
                                        {
                                            c.glideto = (byte)(comhi & 0x1f);
                                        }
                                        else
                                        {
                                            /*AdPlug_LogWrite("CldsPlayer(): unknown command 0x%x encountered!"
                                              " value = 0x%x\n", comhi, comlo);*/
                                        }
                                        break;
                                }
                            }
                            else
                            {
                                byte sound;
                                ushort high;
                                sbyte transp = (sbyte)(transpose & 127);
                                /*
                                 * Originally, in assembler code, the player first shifted
                                 * logically left the transpose byte by 1 and then shifted
                                 * arithmetically right the same byte to achieve the final,
                                 * signed transpose value. Since we can't do arithmetic shifts
                                 * in C, we just duplicate the 7th bit into the 8th one and
                                 * discard the 8th one completely.
                                 */

                                if ((transpose & 64) != 0)
                                {
                                    transp |= -1;   //ED TODO: Is this right? Was |= 128
                                }

                                if ((transpose & 128) != 0)
                                {
                                    sound = (byte)((comlo + transp) & maxsound);
                                    high = (byte)(comhi << 4);
                                }
                                else
                                {
                                    sound = (byte)(comlo & maxsound);
                                    high = (byte)((comhi + transp) << 4);
                                }

                                /*
                              PASCAL:
                                sound = comlo & maxsound;
                                high = (comhi + (((transpose + 0x24) & 0xff) - 0x24)) << 4;
                                */

                                if (chandelay[chan] != 0)
                                {
                                    lds_playsound(sound, chan, high);
                                }
                                else
                                {
                                    c.chancheat.chandelay = chandelay[chan];
                                    c.chancheat.sound = sound;
                                    c.chancheat.high = high;
                                }
                            }
                        }
                    }

                    c.packpos++;
                }
                else
                {
                    c.packwait--;
                }
            }

            tempo_now = tempo;
            /*
              The continue table is updated here, but this is only used in the
              original player, which can be paused in the middle of a song and then
              unpaused. Since AdPlug does all this for us automatically, we don't
              have a continue table here. The continue table update code is noted
              here for reference only.

              if(!pattplay) {
                conttab[speed & maxcont].position = posplay & 0xff;
                conttab[speed & maxcont].tempo = tempo;
              }
            */
            pattplay++;
            if (vbreak)
            {
                pattplay = 0;
                for (i = 0; i < 9; i++)
                {
                    channel[i].packpos = channel[i].packwait = 0;
                }
                posplay = jumppos;
            }
            else
            {
                if (pattplay >= pattlen)
                {
                    pattplay = 0;
                    for (i = 0; i < 9; i++)
                    {
                        channel[i].packpos = channel[i].packwait = 0;
                    }
                    posplay = (ushort)((posplay + 1) & maxpos);
                }
            }
        }
        else
        {
            tempo_now--;
        }

        /* make effects */
        for (chan = 0; chan < 9; chan++)
        {
            c = channel[chan];
            regnum = op_table[chan];
            if (c.keycount > 0)
            {
                if (c.keycount == 1)
                    lds_setregs_adv(0xb0 + chan, 0xdf, 0);
                c.keycount--;
            }

            /* arpeggio */
            if (c.arp_size == 0)
                arpreg = 0;
            else
            {
                arpreg = (ushort)(c.arp_tab[c.arp_pos] << 4);
                if (arpreg == 0x800)
                {
                    if (c.arp_pos > 0) c.arp_tab[0] = c.arp_tab[c.arp_pos - 1];
                    c.arp_size = 1; c.arp_pos = 0;
                    arpreg = (ushort)(c.arp_tab[0] << 4);
                }

                if (c.arp_count == c.arp_speed)
                {
                    c.arp_pos++;
                    if (c.arp_pos >= c.arp_size) c.arp_pos = 0;
                    c.arp_count = 0;
                }
                else
                    c.arp_count++;
            }

            /* glide & portamento */
            if (c.lasttune != 0 && (c.lasttune != c.gototune))
            {
                if (c.lasttune > c.gototune)
                {
                    if (c.lasttune - c.gototune < c.portspeed)
                        c.lasttune = c.gototune;
                    else
                        c.lasttune -= c.portspeed;
                }
                else
                {
                    if (c.gototune - c.lasttune < c.portspeed)
                        c.lasttune = c.gototune;
                    else
                        c.lasttune += c.portspeed;
                }

                if (arpreg >= 0x800)
                    arpreg = (ushort)(c.lasttune - (arpreg ^ 0xff0) - 16);
                else
                    arpreg += c.lasttune;

                freq = frequency[arpreg % (12 * 16)];
                octave = (ushort)(arpreg / (12 * 16) - 1);
                lds_setregs(0xa0 + chan, freq & 0xff);
                lds_setregs_adv(0xb0 + chan, 0x20, ((octave << 2) + (freq >> 8)) & 0xdf);
            }
            else
            {
                /* vibrato */
                if (c.vibwait == 0)
                {
                    if (c.vibrate != 0)
                    {
                        wibc = (ushort)(vibtab[c.vibcount & 0x3f] * c.vibrate);

                        if ((c.vibcount & 0x40) == 0)
                            tune = (ushort)(c.lasttune + (wibc >> 8));
                        else
                            tune = (ushort)(c.lasttune - (wibc >> 8));

                        if (arpreg >= 0x800)
                            tune = (ushort)(tune - (arpreg ^ 0xff0) - 16);
                        else
                            tune += arpreg;

                        freq = frequency[tune % (12 * 16)];
                        octave = (ushort)(tune / (12 * 16) - 1);
                        lds_setregs(0xa0 + chan, freq & 0xff);
                        lds_setregs_adv(0xb0 + chan, 0x20, ((octave << 2) + (freq >> 8)) & 0xdf);
                        c.vibcount += c.vibspeed;
                    }
                    else if (c.arp_size != 0)
                    {   /* no vibrato, just arpeggio */
                        if (arpreg >= 0x800)
                            tune = (ushort)(c.lasttune - (arpreg ^ 0xff0) - 16);
                        else
                            tune = (ushort)(c.lasttune + arpreg);

                        freq = frequency[tune % (12 * 16)];
                        octave = (ushort)(tune / (12 * 16) - 1);
                        lds_setregs(0xa0 + chan, freq & 0xff);
                        lds_setregs_adv(0xb0 + chan, 0x20, ((octave << 2) + (freq >> 8)) & 0xdf);
                    }
                }
                else
                {   /* no vibrato, just arpeggio */
                    c.vibwait--;

                    if (c.arp_size != 0)
                    {
                        if (arpreg >= 0x800)
                            tune = (ushort)(c.lasttune - (arpreg ^ 0xff0) - 16);
                        else
                            tune = (ushort)(c.lasttune + arpreg);

                        freq = frequency[tune % (12 * 16)];
                        octave = (ushort)(tune / (12 * 16) - 1);
                        lds_setregs(0xa0 + chan, freq & 0xff);
                        lds_setregs_adv(0xb0 + chan, 0x20, ((octave << 2) + (freq >> 8)) & 0xdf);
                    }
                }
            }

            /* tremolo (modulator) */
            if (c.trmwait == 0)
            {
                if (c.trmrate != 0)
                {
                    tremc = (ushort)(tremtab[c.trmcount & 0x7f] * c.trmrate);
                    if ((tremc >> 8) <= (c.volmod & 0x3f))
                        level = (byte)((c.volmod & 0x3f) - (tremc >> 8));
                    else
                        level = 0;

                    if (allvolume != 0 && ((fmchip[0xc0 + chan] & 1) != 0))
                        lds_setregs_adv(0x40 + regnum, 0xc0, ((level * allvolume) >> 8) ^ 0x3f);
                    else
                        lds_setregs_adv(0x40 + regnum, 0xc0, level ^ 0x3f);

                    c.trmcount += c.trmspeed;
                }
                else if (allvolume != 0 && ((fmchip[0xc0 + chan] & 1) != 0))
                    lds_setregs_adv(0x40 + regnum, 0xc0, ((((c.volmod & 0x3f) * allvolume) >> 8) ^ 0x3f) & 0x3f);
                else
                    lds_setregs_adv(0x40 + regnum, 0xc0, (c.volmod ^ 0x3f) & 0x3f);
            }
            else
            {
                c.trmwait--;
                if (allvolume != 0 && ((fmchip[0xc0 + chan] & 1) != 0))
                    lds_setregs_adv(0x40 + regnum, 0xc0, ((((c.volmod & 0x3f) * allvolume) >> 8) ^ 0x3f) & 0x3f);
            }

            /* tremolo (carrier) */
            if (c.trcwait == 0)
            {
                if (c.trcrate != 0)
                {
                    tremc = (ushort)(tremtab[c.trccount & 0x7f] * c.trcrate);
                    if ((tremc >> 8) <= (c.volcar & 0x3f))
                        level = (byte)((c.volcar & 0x3f) - (tremc >> 8));
                    else
                        level = 0;

                    if (allvolume != 0)
                        lds_setregs_adv(0x43 + regnum, 0xc0, ((level * allvolume) >> 8) ^ 0x3f);
                    else
                        lds_setregs_adv(0x43 + regnum, 0xc0, level ^ 0x3f);
                    c.trccount += c.trcspeed;
                }
                else if (allvolume != 0)
                    lds_setregs_adv(0x43 + regnum, 0xc0, ((((c.volcar & 0x3f) * allvolume) >> 8) ^ 0x3f) & 0x3f);
                else
                    lds_setregs_adv(0x43 + regnum, 0xc0, (c.volcar ^ 0x3f) & 0x3f);
            }
            else
            {
                c.trcwait--;
                if (allvolume != 0)
                    lds_setregs_adv(0x43 + regnum, 0xc0, ((((c.volcar & 0x3f) * allvolume) >> 8) ^ 0x3f) & 0x3f);
            }
        }

        return (!playing || songlooped) ? false : true;
    }
    public static bool lds_load(BinaryReader f, uint music_offset, uint music_size)
    {
        SoundBank sb;

        f.BaseStream.Seek(music_offset, SeekOrigin.Begin);

        /* load header */
        mode = f.ReadByte();
        if (mode > 2)
        {
            Error.WriteLine("error: failed to load music");
            return false;
        }

        speed = f.ReadUInt16();
        tempo = f.ReadByte();
        pattlen = f.ReadByte();
        chandelay = f.ReadBytes(9);

        regbd = f.ReadByte();

        /* load patches */
        numpatch = f.ReadUInt16();

        soundbank = new SoundBank[numpatch];

        for (int i = 0; i < numpatch; i++)
        {
            sb = soundbank[i] = new SoundBank();
            sb.mod_misc = f.ReadByte();
            sb.mod_vol = f.ReadByte();
            sb.mod_ad = f.ReadByte();
            sb.mod_sr = f.ReadByte();
            sb.mod_wave = f.ReadByte();
            sb.car_misc = f.ReadByte();
            sb.car_vol = f.ReadByte();
            sb.car_ad = f.ReadByte();
            sb.car_sr = f.ReadByte();
            sb.car_wave = f.ReadByte();
            sb.feedback = f.ReadByte();
            sb.keyoff = f.ReadByte();
            sb.portamento = f.ReadByte();
            sb.glide = f.ReadByte();
            sb.finetune = f.ReadByte();
            sb.vibrato = f.ReadByte();
            sb.vibdelay = f.ReadByte();
            sb.mod_trem = f.ReadByte();
            sb.car_trem = f.ReadByte();
            sb.tremwait = f.ReadByte();
            sb.arpeggio = f.ReadByte();
            sb.arp_tab = f.ReadBytes(12);
            sb.start = f.ReadUInt16();
            sb.size = f.ReadUInt16();
            sb.fms = f.ReadByte();
            sb.transp = f.ReadUInt16();
            sb.midinst = f.ReadByte();
            sb.midvelo = f.ReadByte();
            sb.midkey = f.ReadByte();
            sb.midtrans = f.ReadByte();
            sb.middum1 = f.ReadByte();
            sb.middum2 = f.ReadByte();
        }

        /* load positions */
        numposi = f.ReadUInt16();

        positions = new Position[9 * numposi];

        for (int i = 0; i < numposi; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                /*
                * patnum is a pointer inside the pattern space, but patterns are 16bit
                * word fields anyway, so it ought to be an even number (hopefully) and
                * we can just divide it by 2 to get our array index of 16bit words.
                */
                ushort temp = f.ReadUInt16();
                positions[i * 9 + j].patnum = (ushort)(temp / 2);
                positions[i * 9 + j].transpose = f.ReadByte();
            }
        }

        /* load patterns */
        f.BaseStream.Seek(2, SeekOrigin.Current); /* ignore # of digital sounds (dunno what this is for) */

        int remaining = (int)(music_size - (f.BaseStream.Position - music_offset));

        patterns = new ushort[remaining / 2];

        for (int i = 0; i < remaining / 2; i++)
            patterns[i] = f.ReadUInt16();

        lds_rewind();

        return true;
    }

    public static void lds_free()
    {
        soundbank = null;
        positions = null;
        patterns = null;
    }
    public static void lds_rewind()
    {
        /* init all with 0 */
        tempo_now = 3;
        //ED TODO: Music support
        //playing = true; songlooped = false;
        jumping = fadeonoff = allvolume = hardfade = pattplay = 0;
        posplay = jumppos = mainvolume = 0;
        channel = new Channel[channel.Length];
        fmchip = new byte[fmchip.Length];
    }

    private static void lds_playsound(int inst_number, int channel_number, int tunehigh)
    {
        Channel c = channel[channel_number];      /* current channel */
        SoundBank i = soundbank[inst_number];     /* current instrument */
        int regnum = op_table[channel_number];   /* channel's OPL2 register */
        byte volcalc, octave;
        ushort freq;

        /* set fine tune */
        tunehigh += ((i.finetune + c.finetune + 0x80) & 0xff) - 0x80;

        /* arpeggio handling */
        if (i.arpeggio == 0)
        {
            ushort arpcalc = (ushort)(i.arp_tab[0] << 4);

            if (arpcalc > 0x800)
                tunehigh = tunehigh - (arpcalc ^ 0xff0) - 16;
            else
                tunehigh += arpcalc;
        }

        /* glide handling */
        if (c.glideto != 0)
        {
            c.gototune = (ushort)tunehigh;
            c.portspeed = c.glideto;
            c.glideto = c.finetune = 0;
            return;
        }

        /* set modulator registers */
        lds_setregs(0x20 + regnum, i.mod_misc);
        volcalc = i.mod_vol;
        if (c.nextvol == 0 || (i.feedback & 1) == 0)
            c.volmod = volcalc;
        else
            c.volmod = (byte)((volcalc & 0xc0) | ((((volcalc & 0x3f) * c.nextvol) >> 6)));

        if ((i.feedback & 1) == 1 && allvolume != 0)
            lds_setregs(0x40 + regnum, ((c.volmod & 0xc0) | (((c.volmod & 0x3f) * allvolume) >> 8)) ^ 0x3f);
        else
            lds_setregs(0x40 + regnum, c.volmod ^ 0x3f);
        lds_setregs(0x60 + regnum, i.mod_ad);
        lds_setregs(0x80 + regnum, i.mod_sr);
        lds_setregs(0xe0 + regnum, i.mod_wave);

        /* Set carrier registers */
        lds_setregs(0x23 + regnum, i.car_misc);
        volcalc = i.car_vol;
        if (c.nextvol == 0)
            c.volcar = volcalc;
        else
            c.volcar = (byte)((volcalc & 0xc0) | ((((volcalc & 0x3f) * c.nextvol) >> 6)));

        if (allvolume != 0)
            lds_setregs(0x43 + regnum, ((c.volcar & 0xc0) | (((c.volcar & 0x3f) * allvolume) >> 8)) ^ 0x3f);
        else
            lds_setregs(0x43 + regnum, c.volcar ^ 0x3f);
        lds_setregs(0x63 + regnum, i.car_ad);
        lds_setregs(0x83 + regnum, i.car_sr);
        lds_setregs(0xe3 + regnum, i.car_wave);
        lds_setregs(0xc0 + channel_number, i.feedback);
        lds_setregs_adv(0xb0 + channel_number, 0xdf, 0);        /* key off */

        freq = frequency[tunehigh % (12 * 16)];
        octave = (byte)(tunehigh / (12 * 16) - 1);
        if (i.glide == 0)
        {
            if (i.portamento == 0 || c.lasttune == 0)
            {
                lds_setregs(0xa0 + channel_number, freq & 0xff);
                lds_setregs(0xb0 + channel_number, (octave << 2) + 0x20 + (freq >> 8));
                c.lasttune = c.gototune = (ushort)tunehigh;
            }
            else
            {
                c.gototune = (ushort)tunehigh;
                c.portspeed = i.portamento;
                lds_setregs_adv(0xb0 + channel_number, 0xdf, 0x20); /* key on */
            }
        }
        else
        {
            lds_setregs(0xa0 + channel_number, freq & 0xff);
            lds_setregs(0xb0 + channel_number, (octave << 2) + 0x20 + (freq >> 8));
            c.lasttune = (ushort)tunehigh;
            c.gototune = (ushort)(tunehigh + ((i.glide + 0x80) & 0xff) - 0x80); /* set destination */
            c.portspeed = i.portamento;
        }

        if (i.vibrato == 0)
            c.vibwait = c.vibspeed = c.vibrate = 0;
        else
        {
            c.vibwait = i.vibdelay;
            /* PASCAL:    c.vibspeed = ((i.vibrato >> 4) & 15) + 1; */
            c.vibspeed = (byte)((i.vibrato >> 4) + 2);
            c.vibrate = (byte)((i.vibrato & 15) + 1);
        }

        if ((c.trmstay & 0xf0) == 0)
        {
            c.trmwait = (byte)((i.tremwait & 0xf0) >> 3);
            /* PASCAL:    c.trmspeed = (i.mod_trem >> 4) & 15; */
            c.trmspeed = (byte)(i.mod_trem >> 4);
            c.trmrate = (byte)(i.mod_trem & 15);
            c.trmcount = 0;
        }

        if ((c.trmstay & 0x0f) == 0)
        {
            c.trcwait = (byte)((i.tremwait & 15) << 1);
            /* PASCAL:    c.trcspeed = (i.car_trem >> 4) & 15; */
            c.trcspeed = (byte)(i.car_trem >> 4);
            c.trcrate = (byte)(i.car_trem & 15);
            c.trccount = 0;
        }

        c.arp_size = (byte)(i.arpeggio & 15);
        c.arp_speed = (byte)(i.arpeggio >> 4);
        System.Array.Copy(i.arp_tab, c.arp_tab, 12);
        c.keycount = i.keyoff;
        c.nextvol = c.glideto = c.finetune = c.vibcount = c.arp_pos = c.arp_count = 0;
    }
}