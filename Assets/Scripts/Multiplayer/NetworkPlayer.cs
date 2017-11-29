using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class NetworkPlayer : Player, IPunObservable {

    public PhotonView view { get; private set; }

    public float transformUpdateDelta;

    Vector2 targetPosition;
    float targetRotation;

    protected override void InitializeComponents() {
        base.InitializeComponents();

        view = GetComponent<PhotonView>();
    }

    protected override void Update() {
        if(view.isMine) base.Update();
    }

    protected override void FixedUpdate() {
        if(view.isMine) base.FixedUpdate();

        if(Time.time % transformUpdateDelta == 0 && !view.isMine) {
            rigidbody.position = targetPosition;
            rigidbody.rotation = targetRotation;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if(stream.isWriting) {
            stream.SendNext(rigidbody.velocity);
            stream.SendNext(rigidbody.angularVelocity);
            stream.SendNext(rigidbody.position);
            stream.SendNext(rigidbody.rotation);
        }
        else {
            Vector2 velocity = (Vector2)stream.ReceiveNext();
            float angularVelocity = (float)stream.ReceiveNext();
            targetPosition = (Vector2)stream.ReceiveNext();
            targetRotation = (float)stream.ReceiveNext();

            rigidbody.velocity = velocity;
            rigidbody.angularVelocity = angularVelocity;
        }
    }
}
