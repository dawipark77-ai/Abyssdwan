using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class GameViewResolutionSetter
{
    private static bool hasAddedResolution = false;

    static GameViewResolutionSetter()
    {
        EditorApplication.update += CheckAndAddResolution;
    }

    static void CheckAndAddResolution()
    {
        // 한 번만 실행되도록 체크
        if (hasAddedResolution)
        {
            EditorApplication.update -= CheckAndAddResolution;
            return;
        }

        // Unity가 완전히 초기화될 때까지 대기
        if (!EditorApplication.isCompiling && !EditorApplication.isUpdating)
        {
            try
            {
                // Game 뷰 해상도 프리셋 추가
                var gameViewSizesType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizes");
                
                if (gameViewSizesType == null)
                {
                    return; // 타입을 찾을 수 없으면 다음 프레임에 다시 시도
                }

                // current 속성을 안전하게 가져오기
                var currentProperty = gameViewSizesType.GetProperty("current", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                
                if (currentProperty == null)
                {
                    return;
                }

                object gameViewSizesInstance = null;
                try
                {
                    gameViewSizesInstance = currentProperty.GetValue(null);
                }
                catch
                {
                    // 싱글톤이 아직 초기화되지 않았으면 다음 프레임에 다시 시도
                    return;
                }
                
                if (gameViewSizesInstance == null)
                {
                    return;
                }

                var getGroupMethod = gameViewSizesType.GetMethod("GetGroup", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (getGroupMethod == null)
                {
                    hasAddedResolution = true;
                    EditorApplication.update -= CheckAndAddResolution;
                    return;
                }

                // Standalone 플랫폼 그룹 (0) 가져오기
                var group = getGroupMethod.Invoke(gameViewSizesInstance, new object[] { 0 });
                
                if (group == null)
                {
                    hasAddedResolution = true;
                    EditorApplication.update -= CheckAndAddResolution;
                    return;
                }

                var groupType = group.GetType();
                var addCustomSizeMethod = groupType.GetMethod("Add", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                
                if (addCustomSizeMethod == null)
                {
                    hasAddedResolution = true;
                    EditorApplication.update -= CheckAndAddResolution;
                    return;
                }

                var sizeType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSizeType");
                var fixedResolutionType = typeof(Editor).Assembly.GetType("UnityEditor.GameViewSize");
                
                if (sizeType == null || fixedResolutionType == null)
                {
                    hasAddedResolution = true;
                    EditorApplication.update -= CheckAndAddResolution;
                    return;
                }

                var fixedResolutionField = sizeType.GetField("FixedResolution");
                if (fixedResolutionField == null)
                {
                    hasAddedResolution = true;
                    EditorApplication.update -= CheckAndAddResolution;
                    return;
                }

                var fixedResolution = System.Activator.CreateInstance(fixedResolutionType, 
                    new object[] { fixedResolutionField.GetValue(null), 1080, 1920, "1080x1920 (Portrait)" });
                
                addCustomSizeMethod.Invoke(group, new object[] { fixedResolution });
                
                hasAddedResolution = true;
                EditorApplication.update -= CheckAndAddResolution;
            }
            catch (System.Exception e)
            {
                // 에러가 발생해도 계속 시도하지 않도록
                hasAddedResolution = true;
                EditorApplication.update -= CheckAndAddResolution;
                Debug.LogWarning($"GameViewResolutionSetter: {e.Message}");
            }
        }
    }
}

