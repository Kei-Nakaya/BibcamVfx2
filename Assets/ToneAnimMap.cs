using UnityEngine;
using UnityEngine.VFX;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Linq;
using Minis;

public sealed class ToneAnimMap : MonoBehaviour
{
    #region Public enum definitions

    public enum Channel
    {
        All = -1,
        Ch1, Ch2, Ch3, Ch4, Ch5, Ch6, Ch7, Ch8,
        Ch9, Ch10, Ch11, Ch12, Ch13, Ch14, Ch15, Ch16
    }

    public enum Source { AllNotes, NoteNumbers, NoteRange }

    #endregion

    #region Public methods

    // Manual note on/off

    public void NoteOn(int note, float velocity)
    {
        OnNoteOn(note, velocity);
    }

    public void NoteOff(int note)
    {
        OnNoteOff(note);
    }

    #endregion

    #region Editable properties

    private Animator anim;
    // Note input
    [SerializeField] Channel _channel = Channel.All;
    [SerializeField] Source _source = Source.AllNotes;
    [SerializeField] int [] _noteNumbers = new [] { 60 };
    [SerializeField] int _lowestNote = 0;
    [SerializeField] int _highestNote = 127;
    [SerializeField] string triggerName;
    [SerializeField] VisualEffect vfx;
    [SerializeField] string vfxTrigger;

    #endregion

    #region Local members

    // A class used to store a state of a note slot
    class NoteSlot
    {
        public int Note { get; set; } = -1;
        public float TimeOn { get; set; } = 1e+6f;
        public float TimeOff { get; set; } = 1e+6f;
    }

    NoteSlot [] _slots;

    // Check if a note matchs the note options.
    bool NoteFilter(MidiNoteControl note)
    {
        var device = (Minis.MidiDevice)note.device;

        if (_channel != Channel.All && (int)_channel != device.channel)
            return false;

        var number = note.noteNumber;

        switch (_source)
        {
        case Source.NoteNumbers:
            foreach (var n in _noteNumbers) if (n == number) return true;
            return false;
        case Source.NoteRange:
            return _lowestNote <= number && number <= _highestNote;
        default: // Source.AllNotes:
            return true;
        }
    }

    #endregion

    #region Delegate functions

    // Device change callback
    // Subscribe the note on/off events if the device has MIDI capability.
    void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        if (change != InputDeviceChange.Added) return;

        var midi = device as Minis.MidiDevice;
        if (midi == null) return;

        Debug.Log($"OnDeviceChange:{device}");

        midi.onWillNoteOn += OnMidiNoteOn;
        midi.onWillNoteOff += OnMidiNoteOff;
    }

    // Enumerate all MIDI devices and subscribe/unsubscribe them.
    void EditMidiDeviceSubscription(bool flag)
    {
        foreach (var dev in InputSystem.devices)
        {
            var midi = dev as Minis.MidiDevice;
            if (midi == null) continue;

            if (flag)
            {
                midi.onWillNoteOn += OnMidiNoteOn;
                midi.onWillNoteOff += OnMidiNoteOff;
            }
            else
            {
                midi.onWillNoteOn -= OnMidiNoteOn;
                midi.onWillNoteOff -= OnMidiNoteOff;
            }
        }
    }

    // Note on callback for MIDI device
    void OnMidiNoteOn(MidiNoteControl note, float velocity)
    {
        if (NoteFilter(note)) OnNoteOn(note.noteNumber, velocity);
    }

    // Note on callback body
    void OnNoteOn(int note, float velocity)
    {
        anim.SetTrigger(triggerName);
        if (vfx != null && vfxTrigger != null)
        {
            vfx.SendEvent(triggerName);
        }
    }

    // Note off callback for MIDI device
    void OnMidiNoteOff(MidiNoteControl note)
    {
        if (NoteFilter(note)) OnNoteOff(note.noteNumber);
    }

    // Note off callback body
    void OnNoteOff(int note)
    {
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        anim = GetComponent<Animator>();
        // Subscribe the device event.
        EditMidiDeviceSubscription(true);
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    void OnDestroy()
    {
        // Unsubscribe the Input System.
        InputSystem.onDeviceChange -= OnDeviceChange;
        EditMidiDeviceSubscription(false);
    }

    void Update()
    {
    }

    #endregion
}
