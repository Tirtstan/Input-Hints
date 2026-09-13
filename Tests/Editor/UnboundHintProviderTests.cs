using System.Collections.Generic;
using System.Reflection;
using InputHints.Providers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputHints.Tests
{
    public class UnboundHintProviderTests
    {
        private readonly List<Object> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            HintManager.ClearProviders();

            for (int i = 0; i < createdObjects.Count; i++)
                Object.DestroyImmediate(createdObjects[i]);

            createdObjects.Clear();
        }

        [Test]
        public void DeviceProviderRejectsDifferentDeviceLayout()
        {
            Sprite keyboardSprite = CreateSprite();
            HintMapSO keyboardMap = CreateMap("space", keyboardSprite);
            var provider = new DeviceHintProvider<Keyboard>();
            provider.HintMaps.Add(keyboardMap);

            bool resolved = provider.TryGetHintUnbound("<Mouse>/space", out Sprite sprite);

            Assert.That(resolved, Is.False);
            Assert.That(sprite, Is.Null);
        }

        [Test]
        public void GenericGamepadPathDoesNotSearchBrandedMaps()
        {
            Sprite xboxSprite = CreateSprite();
            HintMapSO emptyFallbackMap = CreateMap("buttonSouth", null);
            HintMapSO xboxMap = CreateMap("leftStick", xboxSprite);
            var provider = new GamepadHintProvider(emptyFallbackMap, xboxMap, null, null);

            bool resolved = provider.TryGetHintUnbound("<Gamepad>/leftStick", out Sprite sprite);

            Assert.That(resolved, Is.False);
            Assert.That(sprite, Is.Null);
        }

        [Test]
        public void ExplicitXInputPathUsesXboxMap()
        {
            Sprite fallbackSprite = CreateSprite();
            Sprite xboxSprite = CreateSprite();
            HintMapSO fallbackMap = CreateMap("buttonSouth", fallbackSprite);
            HintMapSO xboxMap = CreateMap("buttonSouth", xboxSprite);
            var provider = new GamepadHintProvider(fallbackMap, xboxMap, null, null);

            bool resolved = provider.TryGetHintUnbound("<XInputController>/buttonSouth", out Sprite sprite);

            Assert.That(resolved, Is.True);
            Assert.That(sprite, Is.SameAs(xboxSprite));
        }

        [Test]
        public void ManagerPreservesLayoutDuringParentFallback()
        {
            Sprite dpadSprite = CreateSprite();
            HintMapSO fallbackMap = CreateMap("dpad", dpadSprite);
            var provider = new GamepadHintProvider(fallbackMap, null, null, null);
            HintManager.RegisterProvider(provider);

            bool resolved = HintManager.TryGetHintUnbound("<Gamepad>/dpad/left", out Sprite sprite);

            Assert.That(resolved, Is.True);
            Assert.That(sprite, Is.SameAs(dpadSprite));
        }

        private HintMapSO CreateMap(string controlPath, Sprite sprite)
        {
            HintMapSO map = ScriptableObject.CreateInstance<HintMapSO>();
            createdObjects.Add(map);

            var entries = new[]
            {
                new HintMapSO.HintEntry { ControlPath = controlPath, Glyph = sprite },
            };
            FieldInfo entriesField = typeof(HintMapSO).GetField(
                "entries",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            Assert.That(entriesField, Is.Not.Null);
            entriesField.SetValue(map, entries);

            return map;
        }

        private Sprite CreateSprite()
        {
            var texture = new Texture2D(1, 1);
            createdObjects.Add(texture);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.zero);
            createdObjects.Add(sprite);
            return sprite;
        }
    }
}
