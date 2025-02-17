import 'normalize.css';
import './dompart/index.scss';
import "@lib/Spector.js/distt/spector.bundle.js";

import { invokerResourcesPretreat } from './invoker-webglpart/invokerResources.js';
import { invokerInitialize3d } from './invoker-webglpart/index.js';

import { toggleHelper } from './invoker-webglpart/toggleHelper.js';
import { toggleLoopState } from './invoker-webglpart/toggleLoopState.js';
import { invokerInitializeKeyListening } from './invoker-webglpart/events/invokerEventPipe.js';

import './dompart/index.js';
import { el_viewportContainer, el_viewport1 } from './dompart/index.js';

window.addEventListener('load', () => {

    // 请求资源
    invokerResourcesPretreat().then(() => {

        invokerInitializeKeyListening(); // 初始化事件监听

        // 挂载3D资源
        invokerInitialize3d(el_viewportContainer, el_viewport1).then(({ resizeViewport, frameloopMachine }) => {

            // 初始化视口大小
            resizeViewport();
            window.addEventListener('resize', resizeViewport);

            // 显示/关闭 帮助对象
            if (import.meta.env.MODE === 'development') { toggleHelper(false); }
            else { toggleHelper(false); }

            // 开启渲染循环
            frameloopMachine.startLoop();

            // 注册事件
            window.addEventListener('keydown', (e) => {
                if (e.code === 'F9') { toggleLoopState(); } // 暂停/启动
                else if (e.code === 'KeyH') { // 开启/关闭可视助手
                    if (import.meta.env.MODE !== "development") return; // 如果当前不处于开发模式, 不让客户端打开助手
                    toggleHelper();
                }
            });

            if (import.meta.env.MODE === "development") {
                // const spector = new SPECTOR.Spector();
                // spector.displayUI();
            }

        });
    })

});

