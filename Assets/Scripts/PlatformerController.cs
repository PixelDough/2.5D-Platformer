using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pixelplacement;

public class PlatformerController : MonoBehaviour
{

    public Collider collider;
    public Spline attachedSpline;
    public float percentage = 0f;
    private float percentageLast = 0f;

    public float speed = 10f;

    //private Rigidbody rb;
    private CharacterController cc;

    private Vector3 targetPosition = Vector3.zero;

    private AnchorData anchorCurrent;
    private AnchorData anchorNext;
    private int anchorNextDirection = 1;
    private Vector3 anchorNextDirectionVector = Vector3.zero;

    private List<AnchorData> anchorsOnSpline = new List<AnchorData>();

    private Vector2 velocity = Vector3.zero;

    private void Start()
    {
        //rb = GetComponent<Rigidbody>();
        cc = GetComponent<CharacterController>();
        
        // Add all anchors from this spline to the list containing the requested anchor data.
        for (int i = 0; i < attachedSpline.Anchors.Length; i++)
        {
            AnchorData anchorData = new AnchorData(attachedSpline.Anchors[i], i);
            anchorData.visited = false;
            anchorsOnSpline.Add(anchorData);
        }
        //anchorsOnSpline[0].visited = true;

        anchorCurrent = GetClosestAnchor(transform.position);
        cc.enabled = false;
        transform.position = new Vector3(anchorCurrent.positionFlat.x, transform.position.y, anchorCurrent.positionFlat.z);
        cc.enabled = true;
        SetAnchorsVisited(anchorCurrent.index);

        anchorNext = GetNextAnchor();

    }

    private void Update()
    {

        //percentageLast = percentage;
        //attachedSpline.CalculateLength();
        //float closestPointPercent = attachedSpline.ClosestPoint(transform.position, 100);
        //closestPointPercent = Mathf.Clamp(closestPointPercent, 0.0001f, 0.9999f);

        //Vector3 directionToNextPoint = (attachedSpline.GetPosition(closestPointPercent, true) - transform.position);
        //directionToNextPoint.y = 0;
        //directionToNextPoint = directionToNextPoint.normalized;

        ////rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, 1);

        //if (Input.GetAxis("Horizontal") != 0f)
        //{



        //    //float relativeSpeed = (speed * Input.GetAxis("Horizontal")) / attachedSpline.Length;
        //    //percentage += relativeSpeed * Time.deltaTime;

        //    //percentage = Mathf.Clamp(percentage, 0, 1);

        //    cc.Move(attachedSpline.Forward(closestPointPercent) * Input.GetAxis("Horizontal") * speed * Time.deltaTime);


        //}

        //Vector3 splinePosition = attachedSpline.GetPosition(percentage, true);
        //targetPosition = new Vector3(splinePosition.x, transform.position.y, splinePosition.z);


        //Vector3 closestLine = attachedSpline.GetPosition(attachedSpline.ClosestPoint(transform.position, 100));
        //float angleFromPoint = Mathf.Atan2(transform.position.z - closestLine.z, transform.position.x - closestLine.x) * 180 / Mathf.PI;

        //float offsetDistance = Vector3.Distance(transform.position, closestLine) * Mathf.Sin(angleFromPoint);
        ////Debug.Log(angleFromPoint);
        //Debug.Log(offsetDistance);

        //cc.enabled = false;
        //if (Mathf.Abs(offsetDistance) > 0.3)
        //    transform.position -= attachedSpline.Right(attachedSpline.ClosestPoint(transform.position, 100)).normalized * offsetDistance;
        //cc.enabled = true;

        //Debug.DrawLine(splinePosition, targetPosition, Color.white);
        //Debug.DrawLine(attachedSpline.GetPosition(closestPointPercent), attachedSpline.GetPosition(closestPointPercent) + Vector3.up, Color.red);

        ////collider.transform.position = new Vector3(splinePosition.x, rb.position.y, splinePosition.z);
        ////rb.MovePosition(new Vector3(splinePosition.x, rb.position.y, splinePosition.z));
        ////cc.Move(targetPosition - transform.position);

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    //rb.AddForce(Vector3.up * 5, ForceMode.VelocityChange);
        //}


        /// Implementation #2: Anchor based movement.
        //

        Vector3 myPos = transform.position;
        myPos.y = 0;

        float inputX = Input.GetAxis("Horizontal");
        if (inputX != 0f)
        {
            int inputSign = (int)Mathf.Sign(inputX);

            if (inputSign != anchorNextDirection)
            {
                if (inputSign > 0)
                    anchorNext = GetNextAnchor();
                else
                    anchorNext = GetPreviousAnchor();
            }
            anchorNextDirection = inputSign;

            AnchorData closestAnchor = GetClosestAnchor(myPos);
            //Vector3.Distance(transform.position, closestAnchor.Anchor.position) <= .1f || 
            //bool anchorNextAngleCheckDifferent = false;
            //if (anchorNext.anchor)
            //    anchorNextAngleCheckDifferent = (anchorNext.anchor.Anchor.position - myPos).normalized != anchorNextDirectionVector;

            if (anchorNext.anchor != null)
                Debug.DrawRay(anchorNext.positionFlat, Vector3.up * 2, Color.green);

            if (Vector3.Distance(myPos, closestAnchor.positionFlat) <= 0.0000001f || anchorNext.anchor == null)
            {

                //anchorCurrent.anchor = closestAnchor;
                //anchorCurrent.index = closestAnchorID;
                anchorCurrent = closestAnchor;

                int splineDirectionModifier = (attachedSpline.direction == SplineDirection.Forward ? 1 : -1);

                int anchorNextID = anchorCurrent.index + inputSign * splineDirectionModifier;
                if (!attachedSpline.loop)
                    anchorNextID = Mathf.Clamp(anchorNextID, 0, attachedSpline.Anchors.Length - 1);
                anchorNextID = (int)Mathf.Repeat(anchorNextID, attachedSpline.Anchors.Length);

                //anchorNext.anchor = attachedSpline.Anchors[anchorNextID];
                //anchorNext.index = anchorNextID;
                anchorNextDirectionVector = (anchorNext.positionFlat - myPos).normalized;
                //Debug.Log(anchorNextDirectionVector);

            }

            //velocity.x = inputX;

            Vector3 directionToNextPoint = (anchorNext.positionFlat - myPos);
            directionToNextPoint.y = 0;

            // Draw debug line to next point.
            Debug.DrawLine(myPos, myPos + directionToNextPoint, Color.green);

            directionToNextPoint.Normalize();
            int oppositeModifier = anchorNextDirection == inputSign ? 1 : -1;
            Vector3 moveAmount = (directionToNextPoint * speed * Time.deltaTime) * Mathf.Abs(inputX) * oppositeModifier;

            if (Vector3.Distance(myPos, anchorNext.positionFlat) <= moveAmount.magnitude)
            {
                moveAmount = (anchorNext.positionFlat - myPos);

                // Set anchor visited state to the appropriate state based on direction.
                anchorNext.visited = (inputSign > 0);

                if (anchorNext.visited)
                    anchorNext = GetNextAnchor();
                else
                    anchorNext = GetPreviousAnchor();
            }

            cc.Move(moveAmount);
        }
        else
        {
            velocity.x = Mathf.Lerp(velocity.x, 0, 30 * Time.deltaTime);
        }


        // IDEA!!!!!!!!!!!!! Divide player SPEED by the Spline LENGTH to get the "normalized" relative speed.
        // (Update: It worked!)

        // How to get player switching paths to the right position:
        // Raycast straight down from the center of the player. If you hit ground, change path and percentage to the closest point on the path to the raycast HIT position.
        // Potential problem: How do we determine when to switch paths, and when to switch paths, and when you're simply walking or flying above a path you're already on?
        // Potential solution: Store the current path. Always racast down. If the path hit is different than the current path, switch paths.
        // Potentail problem with that potential solution: How do you know you've hit a different path? The paths don't have colliders.
        // Potential solution (maybe): Have a script that the ground always has, and it stores which path the ground piece belongs to. Then, when you hit it with a raycast, it will take the path from that script.


        // NEW IDEA: 
        // Only use straight splines (no curves). 
        // Rather than using percentages and points on the path, just use the "anchors" and find the direction towards the next or previous anchor.
        // If distance to next anchor is less than the normalized direction multiplied by our speed, subtract the distance to the anchor from our speed, and move to the anchor immediately.
        // If there is no next or previous anchor, move in the inverse direction from the current anchor to the opposite direction anchor.



    }


    private void Move()
    {
        

    }


    public AnchorData GetClosestAnchor(Vector3 position)
    {
        AnchorData closestAnchor = null;
        float closestDistance = float.MaxValue;
        for( int i = 0; i < anchorsOnSpline.Count; i++)
        {
            AnchorData thisAnchor = anchorsOnSpline[i];

            float thisDistance = Vector3.Distance(position, thisAnchor.positionFlat);
            
            if (thisDistance < closestDistance) { 
                closestAnchor = thisAnchor;
                closestDistance = thisDistance;
                continue;
            }

        }

        if (closestAnchor != null)
        {

            return closestAnchor;
        }

        return null;
    }


    //public AnchorPair GetClosestAnchor(float percentOnPath)
    //{
    //    AnchorPair anchorPair = new AnchorPair();
    //    float closestDistance = float.MaxValue;
    //    float percentPerAnchor = 1 / (attachedSpline.Anchors.Length - 1);
    //    for (int i = 0; i < attachedSpline.Anchors.Length; i++)
    //    {
    //        float percent = percentPerAnchor * i;
    //        if (percent < percentOnPath) continue;

    //        SplineAnchor thisAnchor = attachedSpline.Anchors[i];
    //        float anchorPercent = attachedSpline.ClosestPoint(thisAnchor.Anchor.position, attachedSpline.Anchors.Length);

    //        float thisDistance = Mathf.Abs(anchorPercent - percentOnPath);

    //        Debug.DrawRay(attachedSpline.GetPosition(anchorPercent, true), Vector3.up, Color.blue);

    //        if (thisDistance < closestDistance)
    //        {
    //            anchorPair.anchor = thisAnchor;
    //            anchorPair.index = i;
    //            closestDistance = thisDistance;
    //            continue;
    //        }

    //    }

    //    if (anchorPair.anchor != null && anchorPair.index != -1)
    //    {
    //        return anchorPair;
    //    }
    //    return null;
    //}


    private AnchorData GetFirstAnchorInDirection(float percentOnPath, bool higher = true)
    {
        AnchorData anchorPair = new AnchorData();
        float percentPerAnchor = 1f / (attachedSpline.Anchors.Length - 1f);
        for (int i = 0; i < attachedSpline.Anchors.Length; i++)
        {
            float percent = percentPerAnchor * i;
            if (percent < percentOnPath) continue;

            SplineAnchor thisAnchor = attachedSpline.Anchors[i];

            anchorPair.anchor = thisAnchor;
            anchorPair.index = i;
            break;
        }

        if (anchorPair.anchor != null && anchorPair.index != -1)
        {
            return anchorPair;
        }
        return null;
    }


    private AnchorData GetNextAnchor()
    {
        foreach(AnchorData data in anchorsOnSpline)
        {
            if (data.visited) continue;
            return data;
        }

        if (attachedSpline.loop)
        {
            foreach (AnchorData data in anchorsOnSpline) data.visited = false;
            return anchorsOnSpline[0];
        }

        return anchorNext;
    }


    private AnchorData GetPreviousAnchor()
    {
        for (int i = anchorsOnSpline.Count - 1; i >= 0; i--)
        {
            AnchorData data = anchorsOnSpline[i];
            if (!data.visited) continue;
            return data;
        }

        if (attachedSpline.loop)
        {
            foreach (AnchorData data in anchorsOnSpline) data.visited = true;
            return anchorsOnSpline[anchorsOnSpline.Count - 1];
        }

        return anchorNext;
    }


    private void SetAnchorsVisited(int max)
    {
        foreach(AnchorData anchorData in anchorsOnSpline)
        {
            if (anchorData.index <= max) anchorData.visited = true;
            else anchorData.visited = false;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        MoveToOtherSplinePath moveToOtherSplinePath = other.GetComponent<MoveToOtherSplinePath>();
        if (moveToOtherSplinePath)
        {
            attachedSpline = moveToOtherSplinePath.targetSpline;
            //percentage = attachedSpline.ClosestPoint(rb.position);
        }
        
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        percentage = percentageLast;
    }


    private void OnDrawGizmos()
    {
        // Draw closest anchor position sphere.
        if (anchorCurrent != null)
        {
            Gizmos.color = Color.red;
            if (anchorCurrent.anchor != null)
            Gizmos.DrawSphere(anchorCurrent.anchor.transform.position, 0.25f);
            Gizmos.color = Color.green;
            if (anchorNext.anchor != null)
                Gizmos.DrawSphere(anchorNext.anchor.transform.position, 0.25f);
        }
    }


    public class AnchorData
    {
        public SplineAnchor anchor = null;
        public int index;
        public bool visited = false;

        public Vector3 positionFlat
        {
            get { return new Vector3(anchor.transform.position.x, 0, anchor.transform.position.z); }
        }

        public AnchorData() { }

        public AnchorData(SplineAnchor _anchor, int _index)
        {
            anchor = _anchor;
            index = _index;
        }
    }


}
