FMOD Audio Importer

<img src="https://imgur.com/XqTjhEU.gif"></img>

Automatically import audio files and create FMOD Events with Single/Multi/Scatterer Instruments depending on file name suffixes. Useful when starting a new project and importing a lot of audio files.

Features:
- Audio File import (.wav, .aiff, .mp3, .flac, .ogg) and auto Event creation with single, multi, scatterer instruments.
- Auto adding a spatializer effect to files containing the "_z" suffix.
- Retaining folder structures, avoiding duplicate files and folders.

Usage: Open FMOD Studio and drag and drop a folder containing audio files to the application:

<img src=https://i.imgur.com/UA4pAcL.png></img>

Examples of file names structures:
- <b>Single Instrument</b>: filename.wav will generate an Event named filename with a Single Instrument.
- <b>Single Instrument with Spatializer</b>: filename_z.wav will generate an Event containing a Single Instrument and a Spatializer effect on the Master Track.
- <b>Multi Instrument</b>: filename_m_1.wav, filename_m_2.wav will generate an Event named filename with a Multi Instrument containing both files.
- <b>Multi Instrument with Spatializer</b>: filename_z_m_1.wav, filename_z_m_2.wav will generate an Event named filename with a Multi Instrument containing both files and a Spatializer Effect on the Master Track.
- <b>Scatter Instrument</b>: filename_c_1.wav, filename_c_2.wav will generate an Event named filename with a Scatterer Instrument containing both files.
- <b>Scatter Instrument with Spatializer</b>: filename_z_c_1.wav, filename_z_c_2.wav will generate an Event named filename with a Scatterer Instrument containing both files and a Spatializer Effect on the Master Track.

You can change the suffix settings in the Settings tab:

<img src="https://imgur.com/muPWmgJ.png"></img>
