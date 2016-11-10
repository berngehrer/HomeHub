using System;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace I2CLcd
{
    class LiquidCrystal_I2C
    {
        Stopwatch sw;
        I2cDevice _innerClient;

        // commands
        const byte LCD_CLEARDISPLAY = 0x01;
        const byte LCD_RETURNHOME = 0x02;
        const byte LCD_ENTRYMODESET = 0x04;
        const byte LCD_DISPLAYCONTROL = 0x08;
        const byte LCD_CURSORSHIFT = 0x10;
        const byte LCD_FUNCTIONSET = 0x20;
        const byte LCD_SETCGRAMADDR = 0x40;
        const byte LCD_SETDDRAMADDR = 0x80;

        // flags for display entry mode
        const byte LCD_ENTRYRIGHT = 0x00;
        const byte LCD_ENTRYLEFT = 0x02;
        const byte LCD_ENTRYSHIFTINCREMENT = 0x01;
        const byte LCD_ENTRYSHIFTDECREMENT = 0x00;

        // flags for display on/off control
        const byte LCD_DISPLAYON = 0x04;
        const byte LCD_DISPLAYOFF = 0x00;
        const byte LCD_CURSORON = 0x02;
        const byte LCD_CURSOROFF = 0x00;
        const byte LCD_BLINKON = 0x01;
        const byte LCD_BLINKOFF = 0x00;

        // flags for display/cursor shift
        const byte LCD_DISPLAYMOVE = 0x08;
        const byte LCD_CURSORMOVE = 0x00;
        const byte LCD_MOVERIGHT = 0x04;
        const byte LCD_MOVELEFT = 0x00;

        // flags for function set
        const byte LCD_8BITMODE = 0x10;
        const byte LCD_4BITMODE = 0x00;
        const byte LCD_2LINE = 0x08;
        const byte LCD_1LINE = 0x00;
        const byte LCD_5x10DOTS = 0x04;
        const byte LCD_5x8DOTS = 0x00;

        // flags for backlight control
        const byte LCD_BACKLIGHT = 0x08;
        const byte LCD_NOBACKLIGHT = 0x00;

        const byte En = 0x04;  // Enable bit
        const byte Rw = 0x02;  // Read/Write bit
        const byte Rs = 0x01;  // Register select bit

        int _displayfunction;
        int _displaycontrol;
        int _displaymode;

        byte _Addr;        
        byte _numlines;
        byte _cols;
        byte _rows;
        byte _backlightval;
        

        public LiquidCrystal_I2C(byte lcd_Addr, byte lcd_cols,byte lcd_rows)
        {
          _Addr = lcd_Addr;
          _cols = lcd_cols;
          _rows = lcd_rows;
          _backlightval = LCD_NOBACKLIGHT;
        }

        public async Task Init()
        {
            await init_priv();
        }

        async Task init_priv()
        {
            var dic = await DeviceInformation.FindAllAsync(I2cDevice.GetDeviceSelector());
            var id = dic.Where(x => x.IsEnabled).FirstOrDefault()?.Id;
            _innerClient = await I2cDevice.FromIdAsync(id, new I2cConnectionSettings(0x3F));
            
            
            await Begin(_cols, _rows);
        }

        async Task Begin(byte cols, byte lines, byte dotsize = LCD_5x8DOTS)
        {
            if (lines > 1)
            {
                _displayfunction |= LCD_2LINE;
            }
            _numlines = lines;

            // for some 1 line displays you can select a 10 pixel high font
            if ((dotsize != 0) && (lines == 1))
            {
                _displayfunction |= LCD_5x10DOTS;
            }

            // SEE PAGE 45/46 FOR INITIALIZATION SPECIFICATION!
            // according to datasheet, we need at least 40ms after power rises above 2.7V
            // before sending commands. Arduino can turn on way befer 4.5V so we'll wait 50
            await Task.Delay(50);

            // Now we pull both RS and R/W low to begin commands
            expanderWrite(_backlightval);   // reset expanderand turn backlight off (Bit 8 =1)
            await Task.Delay(1000);

            //put the LCD into 4 bit mode
            // this is according to the hitachi HD44780 datasheet
            // figure 24, pg 46

            // we start in 8bit mode, try to set 4 bit mode
            write4bits(0x03 << 4);
            delayMicroseconds(4500); // wait min 4.1ms

            // second try
            write4bits(0x03 << 4);
            delayMicroseconds(4500); // wait min 4.1ms

            // third go!
            write4bits(0x03 << 4);
            delayMicroseconds(150);

            // finally, set to 4-bit interface
            write4bits(0x02 << 4);

            _displayfunction = LCD_4BITMODE | LCD_1LINE | LCD_5x8DOTS;
            // set # lines, font size, etc.
            command(LCD_FUNCTIONSET | _displayfunction);

            // turn the display on with no cursor or blinking default
            _displaycontrol = LCD_DISPLAYON | LCD_CURSOROFF | LCD_BLINKOFF;
            display();

            // clear it off
            Clear();

            // Initialize to default text direction (for roman languages)
            _displaymode = LCD_ENTRYLEFT | LCD_ENTRYSHIFTDECREMENT;

            // set the entry mode
            command(LCD_ENTRYMODESET | _displaymode);

            Home();
        }

        /********** high level commands, for the user! */
        public void Clear()
        {
            command(LCD_CLEARDISPLAY);// clear display, set cursor position to zero
            delayMicroseconds(2000);  // this command takes a long time!
        }

        public void Home()
        {
            command(LCD_RETURNHOME);  // set cursor position to zero
            delayMicroseconds(2000);  // this command takes a long time!
        }

        public void setCursor(byte col, byte row)
        {
            int[] row_offsets = { 0x00, 0x40, 0x14, 0x54 };
            if (row > _numlines)
            {
                row = (byte)(_numlines - 1);    // we count rows starting w/0
            }
            command(LCD_SETDDRAMADDR | (col + row_offsets[row]));
        }

        // Turn the display on/off (quickly)
        void noDisplay()
        {
            _displaycontrol &= ~LCD_DISPLAYON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }
        void display()
        {
            _displaycontrol |= LCD_DISPLAYON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // Turns the underline cursor on/off
        void noCursor()
        {
            _displaycontrol &= ~LCD_CURSORON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }
        void cursor()
        {
            _displaycontrol |= LCD_CURSORON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // Turn on and off the blinking cursor
        void noBlink()
        {
            _displaycontrol &= ~LCD_BLINKON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }
        void blink()
        {
            _displaycontrol |= LCD_BLINKON;
            command(LCD_DISPLAYCONTROL | _displaycontrol);
        }

        // These commands scroll the display without changing the RAM
        void scrollDisplayLeft()
        {
            command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVELEFT);
        }
        void scrollDisplayRight()
        {
            command(LCD_CURSORSHIFT | LCD_DISPLAYMOVE | LCD_MOVERIGHT);
        }

        // This is for text that flows Left to Right
        void leftToRight()
        {
            _displaymode |= LCD_ENTRYLEFT;
            command(LCD_ENTRYMODESET | _displaymode);
        }

        // This is for text that flows Right to Left
        void rightToLeft()
        {
            _displaymode &= ~LCD_ENTRYLEFT;
            command(LCD_ENTRYMODESET | _displaymode);
        }

        // This will 'right justify' text from the cursor
        void autoscroll()
        {
            _displaymode |= LCD_ENTRYSHIFTINCREMENT;
            command(LCD_ENTRYMODESET | _displaymode);
        }

        // This will 'left justify' text from the cursor
        void noAutoscroll()
        {
            _displaymode &= ~LCD_ENTRYSHIFTINCREMENT;
            command(LCD_ENTRYMODESET | _displaymode);
        }

        // Allows us to fill the first 8 CGRAM locations
        // with custom characters
        //void createChar(byte location, byte charmap[])
        //{
        //    location &= 0x7; // we only have 8 locations 0-7
        //    command(LCD_SETCGRAMADDR | (location << 3));
        //    for (int i = 0; i < 8; i++)
        //    {
        //        write(charmap[i]);
        //    }
        //}

        //createChar with PROGMEM input
        //void createChar(byte location, string charmap)
        //{
        //    location &= 0x7; // we only have 8 locations 0-7
        //    command(LCD_SETCGRAMADDR | (location << 3));
        //    for (int i = 0; i < 8; i++)
        //    {
        //        write(pgm_read_byte_near(charmap++));
        //    }
        //}

        // Turn the (optional) backlight off/on
        void noBacklight()
        {
            _backlightval = LCD_NOBACKLIGHT;
            expanderWrite(0);
        }

        void backlight()
        {
            _backlightval = LCD_BACKLIGHT;
            expanderWrite(0);
        }



        /*********** mid level commands, for sending data/cmds */

        void command(int value)
        {
            send((byte)value, 0);
        }

        void delayMicroseconds(int micros)
        {
            sw = Stopwatch.StartNew();
            int i = 0;

            while (sw.ElapsedMilliseconds <= micros)
            {
                if (sw.Elapsed.Ticks % 100 == 0)
                { i++; /* do something*/ }
            }
            sw.Stop();
        }


        /************ low level data pushing commands **********/

        // write either command or data
        void send(byte value, byte mode)
        {
            int highnib = value & 0xf0;
            int lownib = (value << 4) & 0xf0;
            write4bits((highnib) | mode);
            write4bits((lownib) | mode);
        }

        void write4bits(int value)
        {
            expanderWrite(value);
            pulseEnable((byte)value);
        }

        void expanderWrite(int _data)
        {
            _innerClient.Write(new byte[] { ((byte)((_data) | _backlightval)) });

            //Wire.beginTransmission(_Addr);
            //printIIC((_data) | _backlightval);
            //Wire.endTransmission();
        }

        void pulseEnable(byte _data)
        {
            expanderWrite(_data | En);  // En high
            delayMicroseconds(1);       // enable pulse must be >450ns

            expanderWrite(_data & ~En); // En low
            delayMicroseconds(50);      // commands need > 37us to settle
        }
    }
}