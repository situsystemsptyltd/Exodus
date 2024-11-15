using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

    class SourceFileConverter
    {
        public static string TransformCode(string unityScript)
            {
            var tree = CSharpSyntaxTree.ParseText(unityScript);
            var root = tree.GetRoot();

            // Apply transformations to unity using declarations
            root = TransformUnityUsingDirectives(root);

            // Apply transformations to class declarations
            root = root.ReplaceNodes(
                root.DescendantNodes().OfType<ClassDeclarationSyntax>(),
                (node, _) => TransformMonoBehaviour(node)
            );

            // Apply transformations to methods (e.g., Input, GameObject, etc.)
            root = root.ReplaceNodes(
                root.DescendantNodes().OfType<MethodDeclarationSyntax>(),
                (node, _) => TransformMethod(node)
            );

            return root.NormalizeWhitespace().ToFullString();
        }

    // Replace Unity using directives with Stride equivalents
    private static SyntaxNode TransformUnityUsingDirectives(SyntaxNode root)
    {
        // Find all using directives in the syntax tree
        var usingDirectives = root.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();

        // Create a dictionary to map Unity using directives to Stride equivalents
        var unityToStrideUsings = new (string UnityUsing, string StrideUsing)[]
        {
        ("using UnityEngine;", "Stride.Core.Mathematics"),
        ("using UnityEngine.Rendering;", ""),
        ("using UnityEngine.Profiling;", ""),
        ("using UnityEditor;", ""),  // Stride has no direct equivalent; remove or keep if needed
        ("using Unity.Entities;", "Stride.Entities"),
        ("using Unity.Collections;", "Stride.Collections"),
            // Add more Unity-specific mappings as needed
        };

        var transformedRoot = root.ReplaceNodes(usingDirectives, (oldNode, newNode) =>
        {
            var usingDirective = oldNode as UsingDirectiveSyntax;
            if (usingDirective != null)
            {
                // Get the exact text of the using directive
                var usingText = usingDirective.ToString().Trim();

                // Check if this is a Unity-specific using directive
                var matchingMapping = unityToStrideUsings.FirstOrDefault(mapping => mapping.UnityUsing == usingText);

                if (matchingMapping != default)
                {
                    // If there's a match, create a new using directive based on the Stride equivalent
                    var newUsingDirective = matchingMapping.StrideUsing;

                    // If the Stride equivalent is an empty string, remove the using directive
                    if (string.IsNullOrWhiteSpace(newUsingDirective))
                    {
                        return null; // Remove this using directive
                    }

                    // Otherwise, create a new using directive from the Stride equivalent
                    return SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(newUsingDirective))
                        .WithTriviaFrom(usingDirective); // Keep original formatting (comments, newlines, etc.)
                }
            }

            // If no replacement is needed, return the original directive
            return oldNode;
        });

        return transformedRoot;
    }


    private static ClassDeclarationSyntax TransformMonoBehaviour(ClassDeclarationSyntax classNode)
        {
            // Check if the class has a base list
            var baseList = classNode.BaseList;
            if (baseList != null)
            {
                // Remove MonoBehaviour if it's already in the base list
                var newBaseTypes = baseList.Types.Where(type => type.ToString() != "MonoBehaviour").ToList();

                // Add ScriptComponent to the base list
                newBaseTypes.Add(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ScriptComponent")));

                // Create a new BaseList with the modified base types and set it on the class
                classNode = classNode.WithBaseList(SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(newBaseTypes)));
            }
            else
            {
                // If there is no base list, create one with ScriptComponent as the base class
                classNode = classNode.AddBaseListTypes(SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ScriptComponent")));
            }

            return classNode;
        }

        // Example method transformation for input handling
        private static MethodDeclarationSyntax TransformMethod(MethodDeclarationSyntax method)
        {

            if (method.Identifier.Text == "Awake")
            {
                // Transform Awake to Start (as an example)
                method = method.WithIdentifier(SyntaxFactory.Identifier("Start"));
            }

            if (method.Identifier.Text == "Update")
            {
                // Update method stays as Update in Stride, but you can modify it further
            }


            if (method.Body != null)
            {

                // Get the body of the method as a string to apply transformations.
                var body = method.Body.ToString();

                // Transform Unity Input API (e.g., Input.GetKeyDown(KeyCode.Space) -> Keyboard.IsKeyPressed(Keys.Space))
                body = body.Replace("Input.GetKeyDown(KeyCode.Space)", "Keyboard.IsKeyPressed(Keys.Space)");
                body = body.Replace("Input.GetKey(KeyCode.Space)", "Keyboard.IsKeyDown(Keys.Space)");
                body = body.Replace("Input.GetKeyUp(KeyCode.Space)", "Keyboard.IsKeyReleased(Keys.Space)");

                // Transform GameObject API (e.g., GameObject.Find("name") -> EntityManager.Get())
                body = body.Replace("GameObject.Find(", "EntityManager.Get()");

                // Transform AddComponent() (e.g., gameObject.AddComponent<Rigidbody>() -> entity.Add(new RigidbodyComponent()))
                body = body.Replace("AddComponent<Rigidbody>()", "Add(new RigidbodyComponent())");
                body = body.Replace("AddComponent<BoxCollider>()", "Add(new BoxColliderComponent())");
                body = body.Replace("AddComponent<Camera>()", "Add(new CameraComponent())");

                // Transform Physics-related methods (e.g., OnCollisionEnter -> OnCollision)
                if (body.Contains("OnCollisionEnter"))
                {
                    body = body.Replace("OnCollisionEnter(Collision collision)", "OnCollision(Entity otherEntity)");
                }
                if (body.Contains("OnTriggerEnter"))
                {
                    body = body.Replace("OnTriggerEnter(Collider other)", "OnTrigger(Entity otherEntity)");
                }

                // Transform Unity's lifecycle methods like Awake, Start, Update
                if (body.Contains("Awake()"))
                {
                    body = body.Replace("Awake()", "Start()"); // In Stride, Start() is more commonly used.
                }

                if (body.Contains("Start()"))
                {
                    body = body.Replace("Start()", "OnStart()"); // In Stride, you may want to rename to OnStart()
                }

                if (body.Contains("Update()"))
                {
                    body = body.Replace("Update()", "OnUpdate()"); // Stride uses Update() or OnUpdate()
                }

                // Transform Unity's Camera API (e.g., Camera.main -> Camera.Get())
                body = body.Replace("Camera.main", "Camera.Get()");

                // Transform Unity's time management (e.g., Time.deltaTime -> TimeSpan.Elapsed)
                body = body.Replace("Time.deltaTime", "TimeSpan.Elapsed");

                // Transform Unity's Instantiate and Destroy (e.g., Instantiate(prefab) -> EntityManager.CreateEntity())
                if (body.Contains("Instantiate"))
                {
                    body = body.Replace("Instantiate(", "EntityManager.CreateEntity(");
                }

                if (body.Contains("Destroy"))
                {
                    body = body.Replace("Destroy(", "EntityManager.DestroyEntity(");
                }

                // Transform Unity's UI components (e.g., Button.onClick.AddListener() -> UIComponent.Create<Button>())
                if (body.Contains("Button.onClick.AddListener"))
                {
                    body = body.Replace("Button.onClick.AddListener", "Button.OnClick += ");
                }

                // Transform Unity’s Raycasting (e.g., Physics.Raycast() -> SceneSystem.Raycast())
                body = body.Replace("Physics.Raycast(", "SceneSystem.Raycast(");

                // Transform Unity's Debug.Log -> Stride’s Debug
                body = body.Replace("Debug.Log(", "Debug.WriteLine(");

                // Transform Unity's Animator component to Stride equivalent (this is an example for a simple transition)
                body = body.Replace("Animator.Play(", "AnimationComponent.Play(");

                // Transform Unity's AudioSource.Play() to Stride’s Sound API (example for SoundEmitter)
                if (body.Contains("AudioSource.Play()"))
                {
                    body = body.Replace("AudioSource.Play()", "SoundEmitter.Play()");
                }

                // Transform Unity's Mathf functions (e.g., Mathf.Sqrt() -> Math.Sqrt())
                body = body.Replace("Mathf.Sqrt", "Math.Sqrt");
                body = body.Replace("Mathf.Abs", "Math.Abs");
                body = body.Replace("Mathf.Lerp", "MathHelper.Lerp"); // Stride uses MathHelper.Lerp

                // Transform Unity’s Input.mousePosition (Vector3) to Stride's input system (Point)
                if (body.Contains("Input.mousePosition"))
                {
                    body = body.Replace("Input.mousePosition", "Input.MousePosition");
                }

                // Handle Unity’s Input.mouseScrollDelta (Stride doesn't have an equivalent by default)
                if (body.Contains("Input.mouseScrollDelta"))
                {
                    body = body.Replace("Input.mouseScrollDelta", "MouseWheelDelta");
                }

                // Convert Unity's LayerMask to Stride's collision layers (optional, as Stride doesn't use LayerMask directly)
                if (body.Contains("LayerMask"))
                {
                    body = body.Replace("LayerMask", "CollisionLayer");
                }

                // Transform Unity's Rigidbody (e.g., Rigidbody.velocity -> RigidbodyComponent.Velocity)
                body = body.Replace("Rigidbody.velocity", "RigidbodyComponent.Velocity");
                body = body.Replace("Rigidbody.angularVelocity", "RigidbodyComponent.AngularVelocity");

                // Transform Unity's Vector3 to Stride's Vector3 (Stride uses Stride.Core.Mathematics.Vector3)
                body = body.Replace("Vector3", "Vector3"); // Ensure the namespace is correct
                body = body.Replace("Vector3.", "Vector3."); // In case Unity's specific operations need to be handled.

                // Transform Unity's transform.position to Stride's Entity.Position
                body = body.Replace("transform.position", "Entity.Position");

                // Transform Unity's Camera.main.fieldOfView -> Stride's CameraComponent.FieldOfView
                if (body.Contains("Camera.main.fieldOfView"))
                {
                    body = body.Replace("Camera.main.fieldOfView", "CameraComponent.FieldOfView");
                }

                // Transform Unity's `OnGUI()` to Stride’s UI system or custom handler
                if (body.Contains("OnGUI"))
                {
                    body = body.Replace("OnGUI()", "OnDrawUI()"); // Custom handling for UI in Stride
                }

                // Transform Unity's Time.time -> Stride’s TimeSpan.TotalSeconds
                body = body.Replace("Time.time", "TimeSpan.TotalSeconds");

                // If your code uses other Unity-specific classes or methods, add additional transformations here.

                // Return the modified method with the transformed body.
                method = method.WithBody(SyntaxFactory.Block(SyntaxFactory.ParseStatement(body)));
            }
            return method;
        }
    }


