<?php


class LndPrestaPaymentModuleFrontController extends ModuleFrontController
{
    /**
    * @see FrontController::initContent()
    */
    public function initContent()
    {
        parent::initContent();

        $cart = $this->context->cart;

        echo $this->module->generatePayment($cart);
    }
}


