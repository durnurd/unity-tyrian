using System.Collections;
using UnityEngine;
using static UnityEngine.Mathf;
using static LibC;

[RequireComponent(typeof(ParticleSystem))]
[RequireComponent(typeof(AudioSource))]
public class jukebox : MonoBehaviour {
    public ParticleSystem StarContainer;

    public AudioClip[] Songs;

    public GameObject[] Cameras;


    const int starlib_MAX_STARS = 1000;
    const int MAX_TYPES = 14;

    int song_playing = 0;

    struct JE_StarType {
        public float spX, spY, spZ;
    }

    float tempX, tempY;
    JE_StarType[] star = new JE_StarType[starlib_MAX_STARS];

    ParticleSystem.Particle[] particles = new ParticleSystem.Particle[starlib_MAX_STARS];

    byte setup;
    ushort stepCounter;

    ushort nsp2;
    sbyte nspVar2Inc;

    /* JE: new sprite pointer */
    float nsp;
    float nspVarInc;
    float nspVarVarInc;

    ushort changeTime;
    bool doChange;

    bool roundPos;

    short starlib_speed;
    sbyte speedChange;

    byte pColor;

    AudioSource Player;
    void Start() {
        Player = GetComponent<AudioSource>();
        JE_starlib_init();
    }
    
    void Update() {
        JE_starlib_main();
        if (Player.clip == null || Player.time >= Player.clip.length - .1f) {
            play_song(Random.Range(0, Songs.Length));
        }
    }

    void JE_starlib_main() {
        starlib_speed += speedChange;
        for (int i = 0; i < starlib_MAX_STARS; ++i) {
            JE_StarType stars = star[i];
            /* Move star */
            float tempZ = stars.spZ;
            tempX = ((stars.spX / tempZ) + 160);
            tempY = ((stars.spY / tempZ) + 100);
            tempZ -= starlib_speed;


            /* If star is out of range, make a new one */
            if (tempZ <= 0 ||
                tempY <= 0 || tempY > 198 ||
                tempX > 318 || tempX < 1) {
                stars.spZ = 500;

                JE_newStar();

                stars.spX = tempX;
                stars.spY = tempY;
            } else /* Otherwise, update & draw it */
                {
                stars.spZ = tempZ;

                //off = tempX + tempY * 320;


                if (roundPos)
                    particles[i].position = new Vector3((short)tempX, (short)tempY, tempZ);
                else
                    particles[i].position = new Vector3(tempX, tempY, tempZ);
                if (!StarContainer.colorOverLifetime.enabled) {
                    int tempCol = (pColor + (((short)tempZ >> 4) & 31));
                    particles[i].startColor = palette[tempCol];
                } else {
                    particles[i].startColor = new Color32(255, 255, 255, 255);
                }
                particles[i].remainingLifetime = tempZ;// particles[i].startLifetime;

                /* Draw the pixel! */
                /*if (off >= 640 && off < (320 * 200) - 640) {
                    //surf[off] = tempCol;
                    tempCol += 72;
                    surf[off - 1] = tempCol;
                    surf[off + 1] = tempCol;
                    surf[off - 320] = tempCol;
                    surf[off + 320] = tempCol;

                    tempCol += 72;
                    surf[off - 2] = tempCol;
                    surf[off + 2] = tempCol;
                    surf[off - 640] = tempCol;
                    surf[off + 640] = tempCol;
                }*/
            }
            star[i] = stars;
        }
        StarContainer.SetParticles(particles, starlib_MAX_STARS);

        if (doChange) {
            stepCounter++;
            if (stepCounter > changeTime) {
                JE_changeSetup(0);
            }
        }

        if ((mt_rand() % 1000) == 1) {
            nspVarVarInc = mt_rand_1() * 0.01f - 0.005f;
        }

        nspVarInc += nspVarVarInc;
    }

    bool initialized = false;
    void JE_starlib_init() {
        if (!initialized) {
            initialized = true;

            JE_resetValues();
            JE_changeSetup(2);
            doChange = true;

            StarContainer.Emit(starlib_MAX_STARS);
            StarContainer.GetParticles(particles);

            /* RANDOMIZE; */
            for (short x = 0; x < starlib_MAX_STARS; x++) {
                star[x].spX = (short)((mt_rand() % 64000) - 32000);
                star[x].spY = (short)((mt_rand() % 40000) - 20000);
                star[x].spZ = (short)(x + 1) / 2;
            }
        }
    }

    void OnGUI() {
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.UpperCenter;
        
        GUI.Label(new Rect(0, Screen.height - 70, Screen.width, 20), "Press Q to change camera angles. Press ESC to quit the jukebox.", centeredStyle);
        GUI.Label(new Rect(0, Screen.height - 50, Screen.width, 20), "Arrow keys change the song being played.", centeredStyle);
        GUI.Label(new Rect(0, Screen.height - 30, Screen.width, 20), (song_playing + 1) + " " + musicTitle[song_playing], centeredStyle);

        if (Event.current.type == EventType.KeyDown) {
            bool sh = Event.current.shift;
            switch (Event.current.keyCode) {
                case KeyCode.Escape:
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                    break;
                case KeyCode.Equals:
                    starlib_speed++;
                    speedChange = 0;
                    break;
                case KeyCode.Minus:
                    starlib_speed--;
                    speedChange = 0;
                    break;
                case KeyCode.Alpha1:
                case KeyCode.Alpha2:
                case KeyCode.Alpha3:
                case KeyCode.Alpha4:
                case KeyCode.Alpha5:
                case KeyCode.Alpha6:
                case KeyCode.Alpha7:
                case KeyCode.Alpha8:
                case KeyCode.Alpha9:
                    int idx = Event.current.keyCode - KeyCode.Alpha1 + 1;
                    if (idx < 5 && sh)
                        idx += 10;
                    JE_changeSetup((byte)idx);
                    break;
                case KeyCode.Alpha0:
                    JE_changeSetup(10);
                    break;
                case KeyCode.C:
                    JE_resetValues();
                    break;
                case KeyCode.S:
                    nspVarVarInc = mt_rand_1() * 0.01f - 0.005f;
                    break;
                case KeyCode.LeftBracket:
                    if (Event.current.shift)
                        pColor -= 72;
                    else
                        pColor--;
                    break;
                case KeyCode.RightBracket:
                    if (Event.current.shift)
                        pColor += 72;
                    else
                        pColor++;
                    break;
                case KeyCode.BackQuote:
                    doChange = !doChange;
                    break;
                case KeyCode.P:
                    /*wait_noinput(true, false, false); //TODO PAUSE
                    wait_input(true, false, false);*/
                    break;
                case KeyCode.LeftArrow:
                case KeyCode.UpArrow:
                    play_song((song_playing > 0 ? song_playing : musicTitle.Length) - 1);
                    break;
                case KeyCode.Return:
                case KeyCode.RightArrow:
                case KeyCode.DownArrow:
                    play_song((song_playing + 1) % musicTitle.Length);
                    break;
                case KeyCode.R:
                    roundPos = !roundPos;
                    break;
                case KeyCode.Q:
                    for (int i = 0; i < Cameras.Length; ++i) {
                        if (Cameras[i].activeSelf) {
                            Cameras[i].SetActive(false);
                            if (sh)
                                Cameras[(i > 0 ? i : Cameras.Length) - 1].SetActive(true);
                            else
                                Cameras[(i + 1) % Cameras.Length].SetActive(true);
                            break;
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }

    void play_song(int song) {
        song_playing = song;
        Player.clip = Songs[song];
        Player.Play();
    }

    void JE_resetValues() {
        nsp2 = 1;
        nspVar2Inc = 1;
        nspVarInc = 0.1f;
        nspVarVarInc = 0.0001f;
        nsp = 0;
        pColor = 32;
        starlib_speed = 2;
        speedChange = 0;
    }

    void JE_changeSetup(byte setupType) {
        stepCounter = 0;
        changeTime = (ushort)(mt_rand() % 1000);

        if (setupType > 0) {
            setup = setupType;
        } else {
            setup = (byte)(mt_rand() % (MAX_TYPES + 1));
        }

        if (setup == 1) {
            nspVarInc = 0.1f;
        }
        if (nspVarInc > 2.2f) {
            nspVarInc = 0.1f;
        }
    }

    void JE_newStar() {
        if (setup == 0) {
            tempX = (short)((mt_rand() % 64000) - 32000);
            tempY = (short)((mt_rand() % 40000) - 20000);
        } else {
            nsp = nsp + nspVarInc; /* YKS: < lol */
            switch (setup) {
                case 1:
                    tempX = (short)(Sin(nsp / 30) * 20000);
                    tempY = (short)((mt_rand() % 40000) - 20000);
                    break;
                case 2:
                    tempX = (short)(Cos(nsp) * 20000);
                    tempY = (short)(Sin(nsp) * 20000);
                    break;
                case 3:
                    tempX = (short)((short)(Cos(nsp * 15) * 100) * (short)((nsp / 6) % 200));
                    tempY = (short)((short)(Sin(nsp * 15) * 100) * (short)((nsp / 6) % 200));
                    break;
                case 4:
                    tempX = (short)(Sin(nsp / 60) * 20000);
                    tempY = (short)(Cos(nsp) * (short)(Sin(nsp / 200) * 300) * 100);
                    break;
                case 5:
                    tempX = (short)(Sin(nsp / 2) * 20000);
                    tempY = (short)(Cos(nsp) * (short)(Sin(nsp / 200) * 300) * 100);
                    break;
                case 6:
                    tempX = (short)(Sin(nsp) * 40000);
                    tempY = (short)(Cos(nsp) * 20000);
                    break;
                case 8:
                    tempX = (short)(Sin(nsp / 2) * 40000);
                    tempY = (short)(Cos(nsp) * 20000);
                    break;
                case 7:
                    tempX = (short)(mt_rand() % 65535);
                    if ((mt_rand() % 2) == 0) {
                        tempY = (short)((Cos(nsp / 80) * 10000) + 15000);
                    } else {
                        tempY = (short)(50000 - (short)(Cos(nsp / 80) * 13000));
                    }
                    break;
                case 9:
                    nsp2 = (ushort)(nsp2 + nspVar2Inc);
                    if ((nsp2 == 65535) || (nsp2 == 0)) {
                        nspVar2Inc *= -1;
                    }
                    tempX = (short)(Cos(Sin(nsp2 / 10.0f) + (nsp / 500)) * 32000);
                    tempY = (short)(Sin(Cos(nsp2 / 10.0f) + (nsp / 500)) * 30000);
                    break;
                case 10:
                    nsp2 = (ushort)(nsp2 + nspVar2Inc);
                    if ((nsp2 == 65535) || (nsp2 == 0)) {
                        nspVar2Inc *= -1;
                    }
                    tempX = (short)(Cos(Sin(nsp2 / 5.0f) + (nsp / 100)) * 32000);
                    tempY = (short)(Sin(Cos(nsp2 / 5.0f) + (nsp / 100)) * 30000);
                    break; ;
                case 11:
                    nsp2 = (ushort)(nsp2 + nspVar2Inc);
                    if ((nsp2 == 65535) || (nsp2 == 0)) {
                        nspVar2Inc *= -1;
                    }
                    tempX = (short)(Cos(Sin(nsp2 / 1000.0f) + (nsp / 2)) * 32000);
                    tempY = (short)(Sin(Cos(nsp2 / 1000.0f) + (nsp / 2)) * 30000);
                    break;
                case 12:
                    if (nsp != 0) {
                        nsp2 = (ushort)(nsp2 + nspVar2Inc);
                        if ((nsp2 == 65535) || (nsp2 == 0)) {
                            nspVar2Inc *= -1;
                        }
                        tempX = (short)(Cos(Sin(nsp2 / 2.0f) / (Sqrt(Abs(nsp)) / 10.0f + 1) + (nsp2 / 100.0f)) * 32000);
                        tempY = (short)(Sin(Cos(nsp2 / 2.0f) / (Sqrt(Abs(nsp)) / 10.0f + 1) + (nsp2 / 100.0f)) * 30000);
                    }
                    break;
                case 13:
                    if (nsp != 0) {
                        nsp2 = (ushort)(nsp2 + nspVar2Inc);
                        if ((nsp2 == 65535) || (nsp2 == 0)) {
                            nspVar2Inc *= -1;
                        }
                        tempX = (short)(Cos(Sin(nsp2 / 10.0f) / 2 + (nsp / 20)) * 32000);
                        tempY = (short)(Sin(Sin(nsp2 / 11.0f) / 2 + (nsp / 20)) * 30000);
                    }
                    break;
                case 14:
                    nsp2 = (ushort)(nsp2 + nspVar2Inc);
                    tempX = (short)((Sin(nsp) + Cos(nsp2 / 1000.0f) * 3) * 12000);
                    tempY = (short)((Cos(nsp) * 10000) + nsp2);
                    break;
            }
        }
    }

    static readonly byte[] vga_palette = {
      0,   0,   0,   0,   0, 168,   0, 168,   0,   0, 168, 168,
    168,   0,   0, 168,   0, 168, 168,  84,   0, 168, 168, 168,
     84,  84,  84,  84,  84, 252,  84, 252,  84,  84, 252, 252,
    252,  84,  84, 252,  84, 252, 252, 252,  84, 252, 252, 252,
      0,   0,   0,  20,  20,  20,  32,  32,  32,  44,  44,  44,
     56,  56,  56,  68,  68,  68,  80,  80,  80,  96,  96,  96,
    112, 112, 112, 128, 128, 128, 144, 144, 144, 160, 160, 160,
    180, 180, 180, 200, 200, 200, 224, 224, 224, 252, 252, 252,
      0,   0, 252,  64,   0, 252, 124,   0, 252, 188,   0, 252,
    252,   0, 252, 252,   0, 188, 252,   0, 124, 252,   0,  64,
    252,   0,   0, 252,  64,   0, 252, 124,   0, 252, 188,   0,
    252, 252,   0, 188, 252,   0, 124, 252,   0,  64, 252,   0,
      0, 252,   0,   0, 252,  64,   0, 252, 124,   0, 252, 188,
      0, 252, 252,   0, 188, 252,   0, 124, 252,   0,  64, 252,
    124, 124, 252, 156, 124, 252, 188, 124, 252, 220, 124, 252,
    252, 124, 252, 252, 124, 220, 252, 124, 188, 252, 124, 156,
    252, 124, 124, 252, 156, 124, 252, 188, 124, 252, 220, 124,
    252, 252, 124, 220, 252, 124, 188, 252, 124, 156, 252, 124,
    124, 252, 124, 124, 252, 156, 124, 252, 188, 124, 252, 220,
    124, 252, 252, 124, 220, 252, 124, 188, 252, 124, 156, 252,
    180, 180, 252, 196, 180, 252, 216, 180, 252, 232, 180, 252,
    252, 180, 252, 252, 180, 232, 252, 180, 216, 252, 180, 196,
    252, 180, 180, 252, 196, 180, 252, 216, 180, 252, 232, 180,
    252, 252, 180, 232, 252, 180, 216, 252, 180, 196, 252, 180,
    180, 252, 180, 180, 252, 196, 180, 252, 216, 180, 252, 232,
    180, 252, 252, 180, 232, 252, 180, 216, 252, 180, 196, 252,
      0,   0, 112,  28,   0, 112,  56,   0, 112,  84,   0, 112,
    112,   0, 112, 112,   0,  84, 112,   0,  56, 112,   0,  28,
    112,   0,   0, 112,  28,   0, 112,  56,   0, 112,  84,   0,
    112, 112,   0,  84, 112,   0,  56, 112,   0,  28, 112,   0,
      0, 112,   0,   0, 112,  28,   0, 112,  56,   0, 112,  84,
      0, 112, 112,   0,  84, 112,   0,  56, 112,   0,  28, 112,
     56,  56, 112,  68,  56, 112,  84,  56, 112,  96,  56, 112,
    112,  56, 112, 112,  56,  96, 112,  56,  84, 112,  56,  68,
    112,  56,  56, 112,  68,  56, 112,  84,  56, 112,  96,  56,
    112, 112,  56,  96, 112,  56,  84, 112,  56,  68, 112,  56,
     56, 112,  56,  56, 112,  68,  56, 112,  84,  56, 112,  96,
     56, 112, 112,  56,  96, 112,  56,  84, 112,  56,  68, 112,
     80,  80, 112,  88,  80, 112,  96,  80, 112, 104,  80, 112,
    112,  80, 112, 112,  80, 104, 112,  80,  96, 112,  80,  88,
    112,  80,  80, 112,  88,  80, 112,  96,  80, 112, 104,  80,
    112, 112,  80, 104, 112,  80,  96, 112,  80,  88, 112,  80,
     80, 112,  80,  80, 112,  88,  80, 112,  96,  80, 112, 104,
     80, 112, 112,  80, 104, 112,  80,  96, 112,  80,  88, 112,
      0,   0,  64,  16,   0,  64,  32,   0,  64,  48,   0,  64,
     64,   0,  64,  64,   0,  48,  64,   0,  32,  64,   0,  16,
     64,   0,   0,  64,  16,   0,  64,  32,   0,  64,  48,   0,
     64,  64,   0,  48,  64,   0,  32,  64,   0,  16,  64,   0,
      0,  64,   0,   0,  64,  16,   0,  64,  32,   0,  64,  48,
      0,  64,  64,   0,  48,  64,   0,  32,  64,   0,  16,  64,
     32,  32,  64,  40,  32,  64,  48,  32,  64,  56,  32,  64,
     64,  32,  64,  64,  32,  56,  64,  32,  48,  64,  32,  40,
     64,  32,  32,  64,  40,  32,  64,  48,  32,  64,  56,  32,
     64,  64,  32,  56,  64,  32,  48,  64,  32,  40,  64,  32,
     32,  64,  32,  32,  64,  40,  32,  64,  48,  32,  64,  56,
     32,  64,  64,  32,  56,  64,  32,  48,  64,  32,  40,  64,
     44,  44,  64,  48,  44,  64,  52,  44,  64,  60,  44,  64,
     64,  44,  64,  64,  44,  60,  64,  44,  52,  64,  44,  48,
     64,  44,  44,  64,  48,  44,  64,  52,  44,  64,  60,  44,
     64,  64,  44,  60,  64,  44,  52,  64,  44,  48,  64,  44,
     44,  64,  44,  44,  64,  48,  44,  64,  52,  44,  64,  60,
     44,  64,  64,  44,  60,  64,  44,  52,  64,  44,  48,  64,
      0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,
      0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0,   0
    };

    static Color32[] palette;
    static jukebox() {
        palette = new Color32[vga_palette.Length / 3];
        for (int i = 0; i < palette.Length; ++i) {
            palette[i] = new Color32(vga_palette[i * 3 + 0], vga_palette[i * 3 + 1], vga_palette[i * 3 + 2], 255);
        }
    }

    static readonly string[] musicTitle = {
        "Asteroid Dance Part 2",
        "Asteroid Dance Part 1",
        "Buy/Sell Music",
        "CAMANIS",
        "CAMANISE",
        "Deli Shop Quartet",
        "Deli Shop Quartet No. 2",
        "Ending Number 1",
        "Ending Number 2",
        "End of Level",
        "Game Over Solo",
        "Gryphons of the West",
        "Somebody pick up the Gryphone",
        "Gyges, Will You Please Help Me?",
        "I speak Gygese",
        "Halloween Ramble",
        "Tunneling Trolls",
        "Tyrian, The Level",
        "The MusicMan",
        //"The Navigator",  Where is this song? Nobody knows.
        "Come Back to Me, Savara",
        "Come Back again to Savara",
        "Space Journey 1",
        "Space Journey 2",
        "The final edge",
        "START5",
        "Parlance",
        "Torm - The Gathering",
        "TRANSON",
        "Tyrian: The Song",
        "ZANAC3",
        "ZANACS",
        "Return me to Savara",
        "High Score Table",
        "One Mustn't Fall",
        "Sarah's Song",
        "A Field for Mag",
        "Rock Garden",
        "Quest for Peace",
        "Composition in Q",
        "BEER"
    };
}