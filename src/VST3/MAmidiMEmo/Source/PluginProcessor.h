/*
  ==============================================================================

    This file contains the basic framework code for a JUCE plugin processor.

  ==============================================================================
*/

#pragma once

#include <JuceHeader.h>

#include "windows.h"
#include <stdlib.h>
#include <tchar.h>
#include <vector>
#include <shared_mutex>
#include <shlwapi.h>
#include <random>

#include "..\..\..\munt\mt32emu\soxr\src\soxr.h"
#include "rpc/server.h"
#include "rpc/client.h"

//==============================================================================
/**
*/
class MAmidiMEmoAudioProcessor : public juce::AudioProcessor
{
public:
    //==============================================================================
    MAmidiMEmoAudioProcessor();
    ~MAmidiMEmoAudioProcessor() override;

    //==============================================================================
    void prepareToPlay(double sampleRate, int samplesPerBlock) override;
    void releaseResources() override;

#ifndef JucePlugin_PreferredChannelConfigurations
    bool isBusesLayoutSupported(const BusesLayout& layouts) const override;
#endif

    void processBlock(juce::AudioBuffer<float>&, juce::MidiBuffer&) override;

    void processEvents(juce::MidiBuffer& midiMessages);

    void generateSamples(juce::AudioSampleBuffer& buffer);

    //==============================================================================
    juce::AudioProcessorEditor* createEditor() override;
    bool hasEditor() const override;

    //==============================================================================
    const juce::String getName() const override;

    bool acceptsMidi() const override;
    bool producesMidi() const override;
    bool isMidiEffect() const override;
    double getTailLengthSeconds() const override;

    //==============================================================================
    int getNumPrograms() override;
    int getCurrentProgram() override;
    void setCurrentProgram(int index) override;
    const juce::String getProgramName(int index) override;
    void changeProgramName(int index, const juce::String& newName) override;

    //==============================================================================
    void getStateInformation(juce::MemoryBlock& destData) override;
    void setStateInformation(const void* data, int sizeInBytes) override;

private:
    //==============================================================================
    JUCE_DECLARE_NON_COPYABLE_WITH_LEAK_DETECTOR(MAmidiMEmoAudioProcessor)

        std::shared_mutex mtxBuffer;
    std::shared_mutex mtxSoxrBuffer;

    std::vector<int32_t> m_streamBuffer2ch;
    std::vector<unsigned char> saveData;

    void streamUpdatedL(int32_t size);
    void streamUpdatedR(int32_t size);

    rpc::client* m_rpcClient;
    rpc::server* m_rpcSrv;

    bool m_vstCtor;
    bool m_vstInited;
    bool m_vstIsClosed;
    bool m_vstIsSuspend;
    USHORT m_vstPort;
    USHORT m_mamiPort;
    int m_lastSampleFrames;
    int m_sampleFramesBlock;
    char m_mamiPath[MAX_PATH];

    void updateSampleRateCore();
    int isVstDisabled();
    void startRpcServer();
    bool createSharedMemory();

    bool m_streamBufferOverflowed;
    bool silence;

    CHAR* m_cpSharedMemory;
    HANDLE m_hSharedMemory;
    LONG64 eventId;

    juce::AudioProcessorValueTreeState parameters;
    juce::String customText;

    //==============================================================================
protected:
    int m_mami_sample_rate;
    double m_vst_sample_rate = 0.0; // 追加
    soxr_t soxr;

public:
    void initVst();
};
