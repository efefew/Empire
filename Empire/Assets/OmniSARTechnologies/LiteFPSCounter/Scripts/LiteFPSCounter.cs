//
// Lite FPS Counter
//
// Version    : 1.0.0
// Author     : Alex Tuduran
// Copyright  : OmniSAR Technologies
//

#region

using System;
using OmniSARTechnologies.Helper;
using UnityEngine;
using UnityEngine.UI;

#endregion

#if UNITY_EDITOR
#endif

namespace OmniSARTechnologies.LiteFPSCounter
{
    [SelectionBase]
    [DisallowMultipleComponent]
    public class LiteFPSCounter : MonoBehaviour
    {
        public static float UpdateInterval = 0.5f;
        public static float MinTime = 0.000000001f; // equivalent to 1B fps

        /// <summary>
        ///     Reference to a Text component where the dynamic info will be displayed.
        ///     <para></para>
        ///     <para></para>
        ///     Make sure the referenced UI Text component is not expensive to draw and also not
        ///     expensive to update (keep it as simple and efficient as possible).
        /// </summary>
        [Header("GUI")]
        [Tooltip(
            "Reference to a Text component where the dynamic info will be displayed.\n\r\n\r" +
            "Make sure the referenced UI Text component is not expensive to draw and also not " +
            "expensive to update (keep it as simple and efficient as possible)."
        )]
        public Text dynamicInfoText;

        /// <summary>
        ///     Reference to a Text component where the static info will be displayed.
        ///     <para></para>
        ///     <para></para>
        ///     Although this field will rarely be updated, still make sure the referenced UI Text
        ///     component is at least not expensive to draw.
        /// </summary>
        [Tooltip(
            "Reference to a Text component where the static info will be displayed.\n\r\n\r" +
            "Although this field will rarely be updated, still make sure the referenced UI Text " +
            "component is at least not expensive to draw."
        )]
        public Text staticInfoText;

        private int m_AccumulatedFrames;

        private float m_AccumulatedTime;
        private Color m_CPUFieldsColor = ColorHelper.HexStrToColor("#0090CBFF");
        private string m_DynamicConfigurationFormat;

        private Color m_FPSFieldsColor = ColorHelper.HexStrToColor("#80FF00FF");
        private Color m_FPSFluctuationFieldsColor = ColorHelper.HexStrToColor("#DCEC00FF");
        private Color m_FPSMaxFieldsColor = ColorHelper.HexStrToColor("#00A0FFFF");
        private Color m_FPSMinFieldsColor = ColorHelper.HexStrToColor("#FF8400FF");
        private Color m_GPUDetailFieldsColor = ColorHelper.HexStrToColor("#FF3379FF");
        private Color m_GPUFieldsColor = ColorHelper.HexStrToColor("#FF5020FF");
        private float m_LastUpdateTime;
        private string m_StaticInfoDisplay;
        private Color m_SysFieldsColor = ColorHelper.HexStrToColor("#C9D700FF");

        /// <summary>
        ///     Registered frame time within the update interval.
        /// </summary>
        public float frameTime { get; private set; }

        /// <summary>
        ///     Minimum registered frame time within the update interval.
        /// </summary>
        public float minFrameTime { get; private set; }

        /// <summary>
        ///     Maximum registered frame time within the update interval.
        /// </summary>
        public float maxFrameTime { get; private set; }

        /// <summary>
        ///     Fluctuation of the registered frame time within the update interval.
        /// </summary>
        public float frameTimeFlutuation { get; private set; }

        /// <summary>
        ///     Registered framerate within the update interval.
        /// </summary>
        public float frameRate { get; private set; }

        /// <summary>
        ///     Minimum registered framerate within the update interval.
        /// </summary>
        public float minFrameRate { get; private set; }

        /// <summary>
        ///     Maximum registered framerate within the update interval.
        /// </summary>
        public float maxFrameRate { get; private set; }

        /// <summary>
        ///     Framerate fluctuation within the update interval.
        /// </summary>
        public float frameRateFlutuation { get; private set; }

        /// <summary>
        ///     Resets the framerate probing data.
        ///     <para></para>
        ///     <para></para>
        ///     <remarks>
        ///         This does not reset the component's inspector state.
        ///     </remarks>
        /// </summary>
        public void Reset()
        {
            ResetProbingData();

            m_LastUpdateTime = Time.realtimeSinceStartup;
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateFPS();
        }

        private void OnEnable()
        {
            Initialize();
        }

        /// <summary>
        ///     Initializes (and resets) the component.
        ///     <para></para>
        ///     <para></para>
        ///     <remarks>
        ///         The initialization only targets the component's internal data.
        ///     </remarks>
        /// </summary>
        public void Initialize()
        {
            Reset();
            UpdateInternals();
        }

        public void UpdateInternals()
        {
            UpdateStaticContentAndData();
        }

        private void UpdateStaticContentAndData()
        {
            m_DynamicConfigurationFormat = string.Format(
                "{0} FPS {1} ms {2}" + Environment.NewLine +
                "{3} FPS {4} ms {5}" + Environment.NewLine +
                "{6} FPS {7} ms {8}" + Environment.NewLine +
                "{9} FPS {10} ms {11}",
                ColorHelper.ColorText("{0}", m_FPSFieldsColor),
                ColorHelper.ColorText("{1}", m_FPSFieldsColor),
                ColorHelper.ColorText("Σ", m_FPSFieldsColor),
                ColorHelper.ColorText("{2}", m_FPSMinFieldsColor),
                ColorHelper.ColorText("{3}", m_FPSMinFieldsColor),
                ColorHelper.ColorText("⇓", m_FPSMinFieldsColor),
                ColorHelper.ColorText("{4}", m_FPSMaxFieldsColor),
                ColorHelper.ColorText("{5}", m_FPSMaxFieldsColor),
                ColorHelper.ColorText("⇑", m_FPSMaxFieldsColor),
                ColorHelper.ColorText("{6}", m_FPSFluctuationFieldsColor),
                ColorHelper.ColorText("{7}", m_FPSFluctuationFieldsColor),
                ColorHelper.ColorText("∿", m_FPSFluctuationFieldsColor)
            );

            if (!staticInfoText) return;

            staticInfoText.text = string.Format(
                "{0} {1}" + Environment.NewLine +
                "{2} MB VRAM" + Environment.NewLine +
                "{3}" + Environment.NewLine +
                "{4} MB RAM" + Environment.NewLine +
                "{5}",
                ColorHelper.ColorText(SystemInfo.graphicsDeviceName, m_GPUFieldsColor),
                ColorHelper.ColorText("[" + SystemInfo.graphicsDeviceType + "]", m_GPUDetailFieldsColor),
                ColorHelper.ColorText(SystemInfo.graphicsMemorySize.ToString(), m_GPUFieldsColor),
                ColorHelper.ColorText(SystemInfo.processorType, m_CPUFieldsColor),
                ColorHelper.ColorText(SystemInfo.systemMemorySize.ToString(), m_CPUFieldsColor),
                ColorHelper.ColorText(SystemInfo.operatingSystem, m_SysFieldsColor)
            );
        }

        private void UpdateDynamicContent()
        {
            if (!dynamicInfoText) return;

            dynamicInfoText.text = string.Format(
                m_DynamicConfigurationFormat,
                frameRate.ToString("F1"), (frameTime * 1000.0f).ToString("F1"),
                minFrameRate.ToString("F1"), (maxFrameTime * 1000.0f).ToString("F1"),
                maxFrameRate.ToString("F1"), (minFrameTime * 1000.0f).ToString("F1"),
                frameRateFlutuation.ToString("F1"), (frameTimeFlutuation * 1000.0f).ToString("F1")
            );
        }

        private void ResetProbingData()
        {
            minFrameTime = float.MaxValue;
            maxFrameTime = float.MinValue;
            m_AccumulatedTime = 0.0f;
            m_AccumulatedFrames = 0;
        }

        private void UpdateFPS()
        {
            if (!dynamicInfoText) return;

            float deltaTime = Time.unscaledDeltaTime;

            m_AccumulatedTime += deltaTime;
            m_AccumulatedFrames++;

            if (deltaTime < MinTime) deltaTime = MinTime;

            if (deltaTime < minFrameTime) minFrameTime = deltaTime;

            if (deltaTime > maxFrameTime) maxFrameTime = deltaTime;

            float nowTime = Time.realtimeSinceStartup;
            if (nowTime - m_LastUpdateTime < UpdateInterval) return;

            if (m_AccumulatedTime < MinTime) m_AccumulatedTime = MinTime;

            if (m_AccumulatedFrames < 1) m_AccumulatedFrames = 1;

            frameTime = m_AccumulatedTime / m_AccumulatedFrames;
            frameRate = 1.0f / frameTime;

            minFrameRate = 1.0f / maxFrameTime;
            maxFrameRate = 1.0f / minFrameTime;

            frameTimeFlutuation = Mathf.Abs(maxFrameTime - minFrameTime) / 2.0f;
            frameRateFlutuation = Mathf.Abs(maxFrameRate - minFrameRate) / 2.0f;

            UpdateDynamicContent();

            ResetProbingData();
            m_LastUpdateTime = nowTime;
        }
    }
}